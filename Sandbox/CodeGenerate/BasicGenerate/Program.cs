namespace BasicGenerate
{
    using System;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Reflection;

    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp;

    public interface ICalculator
    {
        int Calc(int x, int y);
    }


    public static class Program
    {
        public static void Main(string[] args)
        {
            var source1 = @"
namespace BasicGenerate
{
    public sealed class Calculator1 : ICalculator
    {
        public int Calc(int x, int y)
        {
            return x + y;
        }
    }
}
";
            var source2 = @"
namespace BasicGenerate
{
    public sealed class Calculator2 : ICalculator
    {
        public int Calc(int x, int y)
        {
            return x - y;
        }
    }
}
";

            var syntaxTree1 = CSharpSyntaxTree.ParseText(source1);
            var syntaxTree2 = CSharpSyntaxTree.ParseText(source2);

            var compilation = CSharpCompilation.Create("InMemoryAssembly")
                .WithOptions(new CSharpCompilationOptions(
                    OutputKind.DynamicallyLinkedLibrary,
                    optimizationLevel: OptimizationLevel.Release,
                    allowUnsafe: true))
                .AddReferences(MetadataReference.CreateFromFile(typeof(object).GetTypeInfo().Assembly.Location))
                .AddReferences(MetadataReference.CreateFromFile(typeof(ICalculator).GetTypeInfo().Assembly.Location))
                .AddSyntaxTrees(syntaxTree1)
                .AddSyntaxTrees(syntaxTree2);

            var stream = new MemoryStream();
            var emitResult = compilation.Emit(stream);

            if (!emitResult.Success)
            {
                foreach (var diagnostic in emitResult.Diagnostics.Where(x => x.IsWarningAsError || x.Severity == DiagnosticSeverity.Error))
                {
                    Debug.WriteLine("{0}: {1}", diagnostic.Id, diagnostic.GetMessage());
                }

                return;
            }

            var assembly = Assembly.Load(stream.GetBuffer());

            var type1 = assembly.GetType("BasicGenerate.Calculator1");
            var instance1 = (ICalculator)Activator.CreateInstance(type1);
            var type2 = assembly.GetType("BasicGenerate.Calculator2");
            var instance2 = (ICalculator)Activator.CreateInstance(type2);

            Debug.WriteLine(instance1.Calc(1, 2));
            Debug.WriteLine(instance2.Calc(1, 2));
        }
    }
}
