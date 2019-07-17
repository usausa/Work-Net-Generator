namespace WorkRoslynCodeGen.Generators
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using CodeGeneration.Roslyn;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.CSharp.Syntax;

    public class DuplicateWithSuffixGenerator : ICodeGenerator
    {
        private readonly string suffix;

        public DuplicateWithSuffixGenerator(AttributeData attributeData)
        {
            suffix = (string)attributeData.ConstructorArguments[0].Value;
        }

        public Task<SyntaxList<MemberDeclarationSyntax>> GenerateAsync(TransformationContext context, IProgress<Diagnostic> progress, CancellationToken cancellationToken)
        {
            var results = SyntaxFactory.List<MemberDeclarationSyntax>();

            var applyToClass = (ClassDeclarationSyntax)context.ProcessingNode;

            var copy = applyToClass
                .WithIdentifier(SyntaxFactory.Identifier(applyToClass.Identifier.ValueText + suffix));

            results = results.Add(copy);

            return Task.FromResult(results);
        }
    }
}
