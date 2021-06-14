using Microsoft.CodeAnalysis;

namespace WorkGeneratorProps
{
    [Generator]
    public sealed class TestGenerator : ISourceGenerator
    {
        public void Initialize(GeneratorInitializationContext context)
        {
        }

        public void Execute(GeneratorExecutionContext context)
        {
            context.AnalyzerConfigOptions.GlobalOptions.TryGetValue("build_property.RootNamespace", out var rootNamespace);

            context.AddSource("Namespace.cs", $"// {rootNamespace}");
        }
    }
}
