using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Linq;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Operations;
using Microsoft.CodeAnalysis.Text;

namespace WorkGenerator
{
    [Generator]
    public sealed class ViewRegisterGenerator : ISourceGenerator
    {
        public void Initialize(GeneratorInitializationContext context)
        {
            context.RegisterForSyntaxNotifications(() => new SyntaxReceiver());
        }

        public void Execute(GeneratorExecutionContext context)
        {
            if (context.SyntaxReceiver is not SyntaxReceiver receiver)
            {
                return;
            }

            var viewAttributeSymbol = context.Compilation.GetTypeByMetadataName("WorkLibrary.ViewAttribute");

            foreach (var classDeclarationSyntax in receiver.CandidateClasses)
            {
                var model = context.Compilation.GetSemanticModel(classDeclarationSyntax.SyntaxTree);
                var typeSymbol = ModelExtensions.GetDeclaredSymbol(model, classDeclarationSyntax);

                var attribute = typeSymbol.GetAttributes()
                    .FirstOrDefault(x => x.AttributeClass.Equals(viewAttributeSymbol, SymbolEqualityComparer.Default));
                if (attribute is null)
                {
                    continue;
                }

                var value = attribute.ConstructorArguments[0].Value;
                Debug.WriteLine(value);
            }

            // TODO
        }

        internal class SyntaxReceiver : ISyntaxReceiver
        {
            public List<ClassDeclarationSyntax> CandidateClasses { get; } = new();

            public void OnVisitSyntaxNode(SyntaxNode syntaxNode)
            {
                // 属性があるクラスを候補
                if ((syntaxNode is ClassDeclarationSyntax { AttributeLists: { Count: > 0 } } classDeclarationSyntax))
                {
                    CandidateClasses.Add(classDeclarationSyntax);
                }
            }
        }
    }
}
