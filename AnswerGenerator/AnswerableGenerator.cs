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
//#if DEBUG
//            if (!Debugger.IsAttached)
//            {
//                Debugger.Launch();
//            }
//#endif

            
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
    }
}