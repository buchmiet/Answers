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
using System.Text.Json;


namespace AnswerGenerator
{

    [Generator]
    public partial class AnswerableGenerator : IIncrementalGenerator, ITestableGenerator
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


        public void Execute(SourceProductionContext context, Compilation compilation,
            ImmutableArray<ClassDeclarationSyntax> typeList)
        {
            // uncomment to debug
#if DEBUG
            if (!Debugger.IsAttached)
            {
  //            Debugger.Launch();
            }
#endif


            if (!PrepareHelperMethods())
            {
                // helper methods could not be prepared, abort
                return;
            }

            foreach (var classDeclaration in typeList)
            {
                var model = compilation.GetSemanticModel(classDeclaration.SyntaxTree);

                if (ModelExtensions.GetDeclaredSymbol(model, classDeclaration) is not INamedTypeSymbol classSymbol)
                    continue;


                if (!classSymbol.AllInterfaces.Contains(compilation.GetTypeByMetadataName(InterfaceName)))
                    continue;
              
                if (!_processedClasses.Add(classSymbol.ToDisplayString()))
                {

                    continue;
                }

                ProcessClass(context, classSymbol);
            }


        }

        public static void SerializeAndAppendToFile(INamedTypeSymbol classSymbol, string filePath)
        {
            // Tworzymy obiekt zawierający dane do serializacji
            var classData = new
            {
                Name = classSymbol.Name,
                Namespace = classSymbol.ContainingNamespace.ToString(),
                Fields = classSymbol.GetMembers().OfType<IFieldSymbol>().Select(field => new
                {
                    FieldName = field.Name,
                    FieldType = field.Type.ToString()
                }).ToList(),
                Properties = classSymbol.GetMembers().OfType<IPropertySymbol>().Select(prop => new
                {
                    PropertyName = prop.Name,
                    PropertyType = prop.Type.ToString()
                }).ToList(),
                Methods = classSymbol.GetMembers().OfType<IMethodSymbol>().Select(m => new
                {
                    MethodName = m.Name,
                    ReturnType = m.ReturnType.ToString(),
                    Parameters = m.Parameters.Select(param => new
                    {
                        ParameterName = param.Name,
                        ParameterType = param.Type.ToString()
                    })
                }).ToList()
            };
            // Serializacja do JSON
            var options = new JsonSerializerOptions
            {
                WriteIndented = true // Uprawnia ładne formatowanie JSON-a
            };
            string json = JsonSerializer.Serialize(classData, options);


#if DEBUG
            if (!Debugger.IsAttached)
            {
                Debugger.Launch();
            }
#endif


        }

        private void ProcessClass(SourceProductionContext context, INamedTypeSymbol classSymbol)
        {
            // Find all constructors
           // SerializeAndAppendToFile(classSymbol, "C:\\Use");
            List<IMethodSymbol> constructors = GetConstructors();
            List<ISymbol> answerServiceMembers = GetAnswerServiceMembers(classSymbol);
            string answerServiceMemberName = DefaultAnswerServiceMemberName;
            // uncomment to debug

            switch (answerServiceMembers.Count)
            {
                case > 1:
                    // More than one answer service member found, this class won't be processed
                    var diagnostic = Diagnostic.Create(
                        MultipleAnswerServiceMembersWarning,
                        classSymbol.Locations.FirstOrDefault(), // You can improve this to be more precise
                        classSymbol.Name);
                    return;
             
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
                        m is IFieldSymbol field &&
                        field.Type.ToDisplayString() == ServiceInterface)
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

        private static readonly DiagnosticDescriptor MultipleAnswerServiceMembersWarning = new DiagnosticDescriptor(
            id: "ANSWR001",
            title: "Multiple IAnswerService members found",
            messageFormat: "The class {0} contains multiple IAnswerService members, which might lead to unexpected behavior.",
            category: "AnswerServiceGeneration",
            DiagnosticSeverity.Warning,
            isEnabledByDefault: true);
    }
}