using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

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
                var typeSymbol = model.GetDeclaredSymbol(classDeclarationSyntax);
                if (typeSymbol is null)
                {
                    continue;
                }

                var attribute = typeSymbol.GetAttributes()
                    .FirstOrDefault(x => x.AttributeClass.Equals(viewAttributeSymbol, SymbolEqualityComparer.Default));
                if (attribute is null)
                {
                    continue;
                }

                var args = attribute.ConstructorArguments[0];

                // IsGlobalNamespace
                Debug.WriteLine("-----");
                Debug.WriteLine(typeSymbol.ContainingNamespace.ToDisplayString());
                Debug.WriteLine(typeSymbol.Name);

                // Enumの値の扱い, Valueをキャストするしかないか？、記述のしかたも色々だし
                Debug.WriteLine("-----");
                Debug.WriteLine(args.Type); // WorkLibrary.ViewAttribute
                Debug.WriteLine(args.Type.ContainingNamespace.ToDisplayString());
                Debug.WriteLine(args.Type.Name);
                Debug.WriteLine(args.Value);    // 0
                Debug.WriteLine(args.Value?.GetType());    // Int32 !

                Debug.WriteLine("-----");
                Debug.WriteLine(attribute.AttributeClass.ToDisplayString());    // WorkLibrary.ViewAttribute
            }

            // TODO 追加する名前空間
            // TODO T4
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
