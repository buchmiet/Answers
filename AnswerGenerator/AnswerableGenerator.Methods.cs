using Microsoft.CodeAnalysis;
using System;
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
        private void ProcessClass(SourceProductionContext context, INamedTypeSymbol classSymbol)
        {
            // Find all constructors
            List<IMethodSymbol> constructors = GetConstructors();
            List<ISymbol> answerServiceMembers = GetAnswerServiceMembers(classSymbol);
            string answerServiceMemberName = DefaultAnswerServiceMemberName;

            switch (answerServiceMembers.Count)
            {
                case > 1:
                    // More than one answer service member found, this class won't be processed
                    answerServiceMemberName = answerServiceMembers.First().Name;
                    break;
                case 1:
                {
                    var member = answerServiceMembers.First();
                    answerServiceMemberName = member.Name;
                    break;
                }
                case 0:
                    GenerateAnswerServiceMember(context, classSymbol, answerServiceMemberName);
                    break;
            }

            // For each constructor that does not have IAnswerService parameter, generate an overload
            if (constructors.Count == 0)
            {
                // No constructors declared, generate the constructor
                GenerateConstructorOverload(context, classSymbol, null, answerServiceMemberName);
            }
            else
            {
                foreach (var constructor in constructors.Where(p =>
                             !p.Parameters.Any(q => q.Type.ToDisplayString().EndsWith("IAnswerService"))))
                {
                    GenerateConstructorOverload(context, classSymbol, constructor, answerServiceMemberName);
                }
            }

            // Generate helper methods with the appropriate field/property name
            GenerateHelperMethods(context, classSymbol, answerServiceMemberName);

            List<ISymbol> GetAnswerServiceMembers(INamedTypeSymbol symbol)
            {
                return symbol.GetMembers()
                    .Where(m =>
                        !m.IsStatic &&
                        m is IPropertySymbol prop &&
                        prop.Type.ToDisplayString() == ServiceName &&
                        !prop.Name.Contains("k__BackingField"))
                    .ToList();
            }

            List<IMethodSymbol> GetConstructors()
            {
                return classSymbol.Constructors
                    .Where(c => !c.IsImplicitlyDeclared &&
                                c.DeclaredAccessibility is Accessibility.Public or Accessibility.Protected
                                    or Accessibility.Internal)
                    .ToList();
            }
        }

        private void GenerateAnswerServiceMember(SourceProductionContext context, INamedTypeSymbol classSymbol,
            string propertyName)
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

        private void GenerateConstructorOverload(SourceProductionContext context, INamedTypeSymbol classSymbol,
            IMethodSymbol? constructor, string answerServiceMemberName)
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
            var constructorSignatureHash =
                constructor?.ToDisplayString(SymbolDisplayFormat.MinimallyQualifiedFormat).GetHashCode() ?? 0;
            context.AddSource($"{className}_ConstructorOverload_{constructorSignatureHash}.g.cs",
                SourceText.From(source, Encoding.UTF8));
        }

        private bool PrepareHelperMethods()
        {
            var assembly = Assembly.GetExecutingAssembly();
            //itearate over all classes in resources
            foreach (var resourceName in ResourceNames)
            {
                using var stream = assembly.GetManifestResourceStream(resourceName);
                if (stream is null)
                {
                    return false;
                }
                var helperClass = GetClassDeclarationSyntax(stream);
                if (helperClass is null)
                {
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

        private void GenerateHelperMethods(SourceProductionContext context, INamedTypeSymbol classSymbol, string answerServiceFieldName)
        {
            var namespaceName = classSymbol.ContainingNamespace.IsGlobalNamespace
                ? null
                : classSymbol.ContainingNamespace.ToDisplayString();

            var className = classSymbol.Name;
            var methodsCode = string.Join("\n\n", _helperMethods).Replace(DefaultAnswerServiceMemberName, answerServiceFieldName);

            var classBody = $"public partial class {className}\n{{\n{methodsCode}\n}}";

            var source = namespaceName == null
                ? classBody
                : $"namespace {namespaceName}\n{{\n{classBody}\n}}";

            context.AddSource($"{className}_HelperMethods.g.cs", SourceText.From(source, Encoding.UTF8));
        }

    }
}
