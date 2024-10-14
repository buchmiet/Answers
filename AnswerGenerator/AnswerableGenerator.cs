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

namespace AnswerGenerator
{
    [Generator]
    public class AnswerableGenerator : IIncrementalGenerator
    {

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

            var iLaunchableSymbol = compilation.GetTypeByMetadataName("Answers.IAnswerable");
            PrepareHelperMethods();

            //// Iteracja przez wszystkie kandydackie klasy
            foreach (var classDeclaration in typeList)
            {
                var model = compilation.GetSemanticModel(classDeclaration.SyntaxTree);
                var classSymbol = ModelExtensions.GetDeclaredSymbol(model, classDeclaration) as INamedTypeSymbol;

                if (classSymbol == null)
                    continue;

                // Logowanie szczegółów klasy
                Debug.WriteLine($"Processing class declaration in file: {classDeclaration.SyntaxTree.FilePath}");
                Debug.WriteLine($"Class declaration: {classDeclaration.ToFullString()}");

                if (!classSymbol.AllInterfaces.Contains(iLaunchableSymbol))
                    continue;

                // Sprawdzanie, czy klasa została już przetworzona
                var classFullName = classSymbol.ToDisplayString();
                if (_processedClasses.Contains(classFullName))
                {
                    Debug.WriteLine($"Class {classFullName} has already been processed. Skipping.");
                    continue;
                }

                // Dodanie klasy do przetworzonych
                _processedClasses.Add(classFullName);

                Debug.WriteLine($"Class {classSymbol.Name} implements ILaunchable. Proceeding with processing.");

                // Przetwarzanie klasy
            
                ProcessClass(context, classSymbol);

                Debug.WriteLine($"Finished processing class {classSymbol.Name}.");
            }
        }

        private void ProcessClass(SourceProductionContext context, INamedTypeSymbol classSymbol)
        {
            // Find all constructors
            var constructors = classSymbol.Constructors
                .Where(c => !c.IsImplicitlyDeclared &&
                            c.DeclaredAccessibility is Accessibility.Public or Accessibility.Protected or Accessibility.Internal)
                .ToList();


            var membersDetails = new StringBuilder();

            // Iterate over all members of the class symbol
            foreach (var member in classSymbol.GetMembers())
            {
                membersDetails.AppendLine($"Member Name: {member.Name}");
                membersDetails.AppendLine($"Member Kind: {member.Kind}");
                membersDetails.AppendLine($"Is Static: {member.IsStatic}");

                // Handle properties and fields differently
                if (member is IFieldSymbol field)
                {
                    membersDetails.AppendLine($"Field Type: {field.Type.ToDisplayString()}");
                }
                else if (member is IPropertySymbol prop)
                {
                    membersDetails.AppendLine($"Property Type: {prop.Type.ToDisplayString()}");
                }

                // Display any additional attributes or modifiers
                if (member is ISymbol symbol)
                {
                    membersDetails.AppendLine($"Declared Accessibility: {symbol.DeclaredAccessibility}");
                }

                membersDetails.AppendLine(); // Add a blank line between members
            }

            var debugOutput = membersDetails.ToString();
            // Find IAnswerService field or property in the class
            // Find IAnswerService property in the class, ignoring backing fields
            var answerServiceMembers = classSymbol.GetMembers()
                .Where(m =>
                    !m.IsStatic &&
                    m is IPropertySymbol prop &&
                    prop.Type.ToDisplayString() == "Answers.IAnswerService" &&
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
                foreach (var constructor in constructors)
                {
                    bool constructorHasAnswerService = constructor.Parameters.Any(p => p.Type.ToDisplayString().EndsWith("IAnswerService"));
                    if (!constructorHasAnswerService)
                    {
                        GenerateConstructorOverload(context, classSymbol, constructor, answerServiceMemberName);
                    }
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
                        public Answers.IAnswerService {{propertyName}} { get; private set; }
                    }
                    """
                : $$"""
                    namespace {{namespaceName}}
                    {
                        public partial class {{className}}
                        {
                            public Answers.IAnswerService {{propertyName}} { get; private set; }
                        }
                    }
                    """;

            context.AddSource($"{className}_AnswerServiceProperty.g.cs", SourceText.From(source, Encoding.UTF8));

            //var testSource = @"
            //namespace TestNamespace { public class TestClass { public void TestMethod() { } }  "; 
            //context.AddSource("TestFile.g.cs", SourceText.From(testSource, Encoding.UTF8));

            //return;

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
    public {className}(Answers.IAnswerService  answerService)
    {{
        {answerServiceMemberName} = answerService;
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
                parameterList += "Answers.IAnswerService answerService";

                // Build argument list (only original parameters)
                var argumentList = string.Join(", ", parameters.Select(p => p.Name));

                // Generate constructor code
                classBody = $@"
public partial class {className}
{{
    public {className}({parameterList})
        : this({argumentList})
    {{
        {answerServiceMemberName} = answerService;
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


        private void PrepareHelperMethods()
        {
            var assembly = Assembly.GetExecutingAssembly();
            var resourceName = "AnswerGenerator.TryAsyncClass.cs";
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
                .FirstOrDefault(c => c.Identifier.Text == "TryAsyncClass");

            if (launchableHelperClass == null)
            {
                // Handle the error: class not found
                return;
            }

            // Collect all method declarations
            var methodSyntaxes = launchableHelperClass.Members
                .OfType<MethodDeclarationSyntax>();


            foreach (var methodSyntax in methodSyntaxes)
            {
                var methodBody = methodSyntax.ToFullString();
                // Modify the method body if needed
             
                _helperMethods.Add(methodBody);
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
}
