using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Text;

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
              //         Debugger.Launch();
            }
#endif

            if (!PrepareHelperMethods(context))
            {
                // helper methods could not be prepared, abort
                return;
            }

            foreach (ClassDeclarationSyntax classDeclaration in typeList)
            {

                var model = compilation.GetSemanticModel(classDeclaration.SyntaxTree);

                if (model.GetDeclaredSymbol(classDeclaration) is not INamedTypeSymbol classSymbol)
                    continue;

                if (!classSymbol.AllInterfaces.Contains(compilation.GetTypeByMetadataName(ClassInterfaceName)))
                    continue;
              
                if (!_processedClasses.Add(classSymbol.ToDisplayString()))
                    continue;

                ProcessClass(context, classSymbol, BuildNestingHierarchy(classSymbol));
            }
        }

     

        private NestingStructure BuildNestingHierarchy(INamedTypeSymbol classSymbol)
        {
            var symbols = new Stack<INamedTypeSymbol>();
            var current = classSymbol;
            while (current is not null)
            {
                symbols.Push(current);
                current = current.ContainingType;
            }

            var returnValue = new NestingStructure();
            while (symbols.Count > 1)
            {
                var symbol = symbols.Pop();
                returnValue.AddSignature(symbol.Name);
            }

            return returnValue;
        }

        public class NestingStructure
        {
            private StringBuilder _opening=new StringBuilder();
            private StringBuilder _closing=new StringBuilder();

            public string Opening => _opening.ToString();
            public string Closing => _closing.ToString();

            public void AddSignature(string signature)
            {
                _opening.AppendLine($"partial class {signature}");
                _opening.AppendLine("{");
                _closing.AppendLine("}");
            }
        }

    }
}
