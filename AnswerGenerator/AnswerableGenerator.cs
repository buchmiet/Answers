using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis.Text;
using Microsoft.CodeAnalysis.CSharp;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Diagnostics;
using AnswerGenerator;
using System.Reflection.Metadata;

namespace AnswerGenerator
{
    public interface ITestableGenerator
    {
        string ServiceName { get; }
        string InterfaceName { get; }
        string ConstructorServiceField { get; }
    }
    }

    [Generator]
    public class AnswerableGenerator : IIncrementalGenerator, ITestableGenerator
{

        public string ServiceName => "Answers.IAnswerService";
        public string InterfaceName => "Answers.IAnswerable";
    public string ConstructorServiceField => "answerService";
    void IIncrementalGenerator.Initialize(IncrementalGeneratorInitializationContext context)
        {
            var provider = context.SyntaxProvider.CreateSyntaxProvider(
                predicate: (c, _) => c is ClassDeclarationSyntax,
                transform: (n, _) => (ClassDeclarationSyntax)n.Node
            ).Where(m => m is not null);




            var compilation = context.CompilationProvider.Combine(provider.Collect());
            context.RegisterSourceOutput(compilation,
                (spc, source) => Execute(spc, source.Left, source.Right));

        }


        private readonly HashSet<string> _processedClasses = [];
        private List<string> _helperMethods = new();
        public void Execute(SourceProductionContext context, Compilation compilation, ImmutableArray<ClassDeclarationSyntax> typeList)
        {
#if DEBUG
            if (!Debugger.IsAttached)
            {
        //  Debugger.Launch();
            }
#endif 

            var iLaunchableSymbol = compilation.GetTypeByMetadataName(InterfaceName);
            PrepareHelperMethods();

            //// Iteracja przez wszystkie kandydackie klasy
            foreach (var classDeclaration in typeList)
            {
                var model = compilation.GetSemanticModel(classDeclaration.SyntaxTree);

            if (ModelExtensions.GetDeclaredSymbol(model, classDeclaration) is not INamedTypeSymbol classSymbol)
                continue;


            if (!classSymbol.AllInterfaces.Contains(iLaunchableSymbol))
                    continue;

                // Sprawdzanie, czy klasa została już przetworzona
                var classFullName = classSymbol.ToDisplayString();
                if (!_processedClasses.Add(classFullName))
                {
                
                    continue;
                }

                // Przetwarzanie klasy
            
                ProcessClass(context, classSymbol);

              
            }
        }

        private void ProcessClass(SourceProductionContext context, INamedTypeSymbol classSymbol)
        {
            // Find all constructors
            var constructors = classSymbol.Constructors
                .Where(c => !c.IsImplicitlyDeclared &&
                            c.DeclaredAccessibility is Accessibility.Public or Accessibility.Protected or Accessibility.Internal)
                .ToList();

            
            // Find IAnswerService property in the class, ignoring backing fields
            var answerServiceMembers = classSymbol.GetMembers()
                .Where(m =>
                    !m.IsStatic &&
                    m is IPropertySymbol prop &&
                    prop.Type.ToDisplayString() == ServiceName &&
                    !prop.Name.Contains("k__BackingField"))
                .ToList();



            string answerServiceMemberName = "_answerService"; // Default name

            if (answerServiceMembers.Count > 0)
            {
                var member = answerServiceMembers.First();
                answerServiceMemberName = member.Name;
            }
            else
            {
            
                // If there is no field/property, we need to add one
                GenerateAnswerServiceMember(context, classSymbol, answerServiceMemberName);
            }

            // For each constructor that does not have IAnswerService parameter, generate an overload
            if (constructors.Count == 0)
            {
                // No constructors declared, generate the constructor
                GenerateConstructorOverload(context, classSymbol, null, answerServiceMemberName);
            }
            else
            {
                foreach (var constructor in constructors.Where(p=>!p.Parameters.Any(q => q.Type.ToDisplayString().EndsWith("IAnswerService"))))
                {
                    GenerateConstructorOverload(context, classSymbol, constructor, answerServiceMemberName);
                }
            }

            // Generate helper methods with the appropriate field/property name
            GenerateHelperMethods(context, classSymbol, answerServiceMemberName);
        }

        private void GenerateAnswerServiceMember(SourceProductionContext context, INamedTypeSymbol classSymbol, string propertyName)
        {
            var namespaceName = classSymbol.ContainingNamespace.IsGlobalNamespace
                ? null
                : classSymbol.ContainingNamespace.ToDisplayString();
            var className = classSymbol.Name;

            var source = namespaceName is null
                ? $$"""
                    public partial class {{className}}
                    {
                         private readonly {{ServiceName}} {{propertyName}};
                    }
                    """
                : $$"""
                    namespace {{namespaceName}}
                    {
                        public partial class {{className}}
                        {
                            private readonly {{ServiceName}} {{propertyName}};
                        }
                    }
                    """;

            context.AddSource($"{className}_AnswerServiceProperty.g.cs", SourceText.From(source, Encoding.UTF8));

        }

        private void GenerateConstructorOverload(SourceProductionContext context, INamedTypeSymbol classSymbol, IMethodSymbol? constructor, string answerServiceMemberName)
        {
            var namespaceName = classSymbol.ContainingNamespace.IsGlobalNamespace
                ? null
                : classSymbol.ContainingNamespace.ToDisplayString();
            var className = classSymbol.Name;

            string classBody;

            if (constructor is null)
            {
                // No constructors found, generate a constructor without calling `this()`
                classBody = $@"
public partial class {className}
{{
    public {className}({ServiceName}  {ConstructorServiceField})
    {{
        {answerServiceMemberName} = {ConstructorServiceField};
    }}
}}";
            }
            else
            {
                var parameters = constructor.Parameters;

                // Build parameter list
                var parameterList = string.Join(", ", parameters.Select(p => $"{p.Type.ToDisplayString()} {p.Name}"));
                if (parameters.Length > 0)
                    parameterList += ", ";
                parameterList += $"{ServiceName} {ConstructorServiceField}";

                // Build argument list (only original parameters)
                var argumentList = string.Join(", ", parameters.Select(p => p.Name));

                // Generate constructor code
                classBody = $@"
public partial class {className}
{{
    public {className}({parameterList})
        : this({argumentList})
    {{
        {answerServiceMemberName} = {ConstructorServiceField};
    }}
}}";
            }

            // Include namespace if it's not global
            var source = namespaceName is null
                ? classBody
                : $@"
namespace {namespaceName}
{{
    {classBody}
}}";

            // Ensure unique filenames for each constructor overload
            var constructorSignatureHash = constructor?.ToDisplayString(SymbolDisplayFormat.MinimallyQualifiedFormat).GetHashCode() ?? 0;
            context.AddSource($"{className}_ConstructorOverload_{constructorSignatureHash}.g.cs", SourceText.From(source, Encoding.UTF8));
        }

        private static readonly List<string> ResourceNames= ["AnswerGenerator.TryAsyncClass.cs", "AnswerGenerator.TryClass.cs"];
        private readonly List<string> _classesInResources = ResourceNames
            .Select(name => name.Split('.')[1]) // Rozdziela ciąg po kropce i wybiera część po pierwszej kropce
            .ToList();

        private void PrepareHelperMethods()
        {
            var assembly = Assembly.GetExecutingAssembly();
            foreach (var resourceName in ResourceNames)
            {

                using var stream = assembly.GetManifestResourceStream(resourceName);
                if (stream == null)
                {
                    // Handle the error: resource not found
                    return;
                }

                using var reader = new StreamReader(stream);
                var sourceCode = reader.ReadToEnd();

                // Parse the source code
                var syntaxTree = CSharpSyntaxTree.ParseText(sourceCode);
                var root = syntaxTree.GetRoot();

                // Find the class declaration for LaunchableHelper
                var launchableHelperClass = root.DescendantNodes()
                    .OfType<ClassDeclarationSyntax>()
                    .FirstOrDefault(c => _classesInResources.Contains(c.Identifier.Text));

                if (launchableHelperClass is null)
                {
                    // Handle the error: class not found
                    return;
                }

                // Collect all method declarations
                var methodSyntaxes = launchableHelperClass.Members
                    .OfType<MethodDeclarationSyntax>();


                foreach (var methodSyntax in methodSyntaxes)
                {
                    _helperMethods.Add(methodSyntax.ToFullString());
                }
            }

          

        }

        private void GenerateHelperMethods(SourceProductionContext context, INamedTypeSymbol classSymbol, string answerServiceFieldName)
        {
            var namespaceName = classSymbol.ContainingNamespace.IsGlobalNamespace
                ? null
                : classSymbol.ContainingNamespace.ToDisplayString();
            var className = classSymbol.Name;

            var methodsCode = string.Join("\n\n", _helperMethods).Replace("_answerService", answerServiceFieldName); ;

            // Generowanie kodu metod
            var classBody = $$"""
                                          public partial class {{className}}
                                          {
                              
                                                  {{methodsCode}}
                              
                                          }
                              """;
            var source = namespaceName is null
                ? classBody
                : $$"""
                    namespace {{namespaceName}}
                    {
                        {{classBody}}
                    }
                    """;

            context.AddSource($"{className}_HelperMethods.g.cs", SourceText.From(source, Encoding.UTF8));
        }


    }
