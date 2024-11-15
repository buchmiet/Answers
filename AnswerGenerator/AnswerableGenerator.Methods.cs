using Microsoft.CodeAnalysis;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis.Text;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.CSharp;
using System.IO;
using System.Reflection;

namespace AnswerGenerator
{
    public partial class AnswerableGenerator
    {
        private void ProcessClass(SourceProductionContext context, INamedTypeSymbol classSymbol,NestingStructure nestingStructure)
        {
            List<IMethodSymbol> constructors = GetConstructors();
            List<ISymbol> answerServiceMembers = GetAnswerServiceMembers(classSymbol);
            string answerServiceMemberName = DefaultAnswerServiceMemberName;

            switch (answerServiceMembers.Count)
            {
                case > 1:
                    // More than one answer service member found, this class won't be processed
                    var memberLocations = answerServiceMembers.Select(m => m.Locations.FirstOrDefault())
                        .Where(loc => loc != null).ToList();

                    foreach (var location in memberLocations)
                    {
                        var diagnostic = Diagnostic.Create(
                            WarningGenerator(Warnings.MultipleAnswerServiceMembers),
                            location,
                            classSymbol.Name);
                        context.ReportDiagnostic(diagnostic);
                    }
                    return;
                case 1:
                {
                    var member = answerServiceMembers.First();
                    answerServiceMemberName = member.Name;
                    break;
                }
                case 0:
                    GenerateAnswerServiceMember(context, classSymbol, answerServiceMemberName,nestingStructure);
                    break;
            }

            // For each constructor that does not have IAnswerService parameter, generate an overload
            if (constructors.Count == 0)
            {
                // No constructors declared, generate the constructor
                GenerateConstructorOverload(context, classSymbol, null, answerServiceMemberName, nestingStructure);
            }
            else
            {
                foreach (var constructor in constructors.Where(p =>
                             !p.Parameters.Any(q => q.Type.ToDisplayString().EndsWith("IAnswerService"))))
                {
                    GenerateConstructorOverload(context, classSymbol, constructor, answerServiceMemberName, nestingStructure);
                }
            }

            // Generate helper methods with the appropriate field/property name
            GenerateHelperMethods(context, classSymbol, answerServiceMemberName,nestingStructure);
            return;

            List<ISymbol> GetAnswerServiceMembers(INamedTypeSymbol symbol)=>
                 symbol.GetMembers()
                    .Where(m =>
                        !m.IsStatic &&
                        m is IFieldSymbol field &&
                        field.Type.ToDisplayString() == ServiceInterface)
                    .ToList();
            
            List<IMethodSymbol> GetConstructors()=>
                classSymbol.Constructors
                    .Where(c => !c.IsImplicitlyDeclared &&
                                c.DeclaredAccessibility is Accessibility.Public or Accessibility.Protected
                                    or Accessibility.Internal)
                    .ToList();
        }

        private void GenerateAnswerServiceMember(SourceProductionContext context, INamedTypeSymbol classSymbol,
            string propertyName,NestingStructure nestingStructure)
        {
            var namespaceName = classSymbol.ContainingNamespace.IsGlobalNamespace
                ? null
                : classSymbol.ContainingNamespace.ToDisplayString();
            var className = classSymbol.Name;
            var source = GenerateAnswerServiceMemberSource(namespaceName, className, propertyName);
            context.AddSource($"{className}_AnswerServiceProperty.g.cs", SourceText.From(nestingStructure.Opening+source+nestingStructure.Closing, Encoding.UTF8));

        }

        private void GenerateConstructorOverload(SourceProductionContext context, INamedTypeSymbol classSymbol,
            IMethodSymbol constructor, string answerServiceMemberName,NestingStructure nestingStructure)
        {
            var namespaceName = classSymbol.ContainingNamespace.IsGlobalNamespace
                ? null
                : classSymbol.ContainingNamespace.ToDisplayString();
            var className = classSymbol.Name;

            string classBody;

            if (constructor is null)
            {
                // No constructors found, generate a constructor without calling `this()`
                classBody = GenerateConstructorOverload_001(className, answerServiceMemberName);
            }
            else
            {
                var parameters = constructor.Parameters;
                // Build parameter list
                var parameterList = string.Join(", ", parameters.Select(p => $"{p.Type.ToDisplayString()} {p.Name}"));
                if (parameters.Length > 0)
                {
                    parameterList += ", ";
                }
                parameterList += $"{ServiceInterface} {ConstructorServiceField}";
                // Build argument list (only original parameters)
                var argumentList = string.Join(", ", parameters.Select(p => p.Name));
                // Generate constructor code
                classBody = GenerateConstructorOverload_002(className, parameterList, argumentList, answerServiceMemberName);
            }

            // Include namespace if it's not global
            var source = GenerateConstructorOverload_003(classBody, namespaceName);

            // Ensure unique filenames for each constructor overload
            var constructorSignatureHash =
                constructor?.ToDisplayString(SymbolDisplayFormat.MinimallyQualifiedFormat).GetHashCode() ?? 0;
            context.AddSource($"{className}_ConstructorOverload_{constructorSignatureHash}.g.cs",
                SourceText.From(nestingStructure.Opening+source+nestingStructure.Closing, Encoding.UTF8));
        }

        private bool PrepareHelperMethods(SourceProductionContext context)
        {
            var assembly = Assembly.GetExecutingAssembly();
            //iterate over all classes in resources
            foreach (var resourceName in ResourceNames)
            {
                using var stream = assembly.GetManifestResourceStream(resourceName);
                if (stream is null)
                {
                    var diagnostic = Diagnostic.Create(
                        WarningGenerator(Warnings.ResourceFileNotFound),
                        Location.None,
                        resourceName);
                    context.ReportDiagnostic(diagnostic);
                    return false;
                }
                var helperClass = GetClassDeclarationSyntax(stream);
                if (helperClass is null)
                {
                    var diagnostic = Diagnostic.Create(
                        WarningGenerator(Warnings.RequiredClassNotFoundInResource),
                        Location.None,
                        _classesInResources.FirstOrDefault());
                    context.ReportDiagnostic(diagnostic);
                    return false;
                }
                _helperMethods.AddRange(ExtractMethodsSourcesFromAclass(helperClass));
            }
            return true;

            ClassDeclarationSyntax GetClassDeclarationSyntax(Stream stream)
            {
                using var reader = new StreamReader(stream);
                var sourceCode = reader.ReadToEnd();
                // Parse the source code
                var syntaxTree = CSharpSyntaxTree.ParseText(sourceCode);
                var root = syntaxTree.GetRoot();

                return root.DescendantNodes()
                    .OfType<ClassDeclarationSyntax>()
                    .FirstOrDefault(c => _classesInResources.Contains(c.Identifier.Text));
            }
            List<string> ExtractMethodsSourcesFromAclass(ClassDeclarationSyntax processedClassDeclarationSyntax)
            {
                IEnumerable<MethodDeclarationSyntax> methodSyntaxes = processedClassDeclarationSyntax.Members
                    .OfType<MethodDeclarationSyntax>();
                return methodSyntaxes.Select(p => p.ToFullString()).ToList();
            }
        }

        private void GenerateHelperMethods(SourceProductionContext context, INamedTypeSymbol classSymbol, string answerServiceFieldName,NestingStructure nestingStructure)
        {
            var namespaceName = classSymbol.ContainingNamespace.IsGlobalNamespace
                ? null
                : classSymbol.ContainingNamespace.ToDisplayString();
            var className = classSymbol.Name;
            var methodsCode = string.Join("\r\n", _helperMethods).Replace(DefaultAnswerServiceMemberName, answerServiceFieldName);
            var classBody = GenerateHelperMethods_001(className, methodsCode);
            var source = GenerateHelperMethods_002(namespaceName, classBody);
            context.AddSource($"{className}_HelperMethods.g.cs", SourceText.From(nestingStructure.Opening+source+nestingStructure.Closing, Encoding.UTF8));
        }

    }
}
