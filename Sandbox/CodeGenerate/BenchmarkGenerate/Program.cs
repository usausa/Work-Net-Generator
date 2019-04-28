using System.IO;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace BenchmarkGenerate
{
    using System;
    using System.Reflection;

    using BenchmarkDotNet.Attributes;
    using BenchmarkDotNet.Configs;
    using BenchmarkDotNet.Diagnosers;
    using BenchmarkDotNet.Exporters;
    using BenchmarkDotNet.Jobs;
    using BenchmarkDotNet.Running;

    public class Program
    {
        public static void Main(string[] args)
        {
            BenchmarkSwitcher.FromAssembly(typeof(Program).GetTypeInfo().Assembly).Run(args);
        }
    }

    public class BenchmarkConfig : ManualConfig
    {
        public BenchmarkConfig()
        {
            Add(MarkdownExporter.Default, MarkdownExporter.GitHub);
            Add(MemoryDiagnoser.Default);
            Add(Job.LongRun);
        }
    }

    [Config(typeof(BenchmarkConfig))]
    public class Benchmark
    {
        private IFunction preCompiled;

        private IFunction dynamicCompiled;

        [GlobalSetup]
        public void Setup()
        {
            preCompiled = new Function();

            var source = @"
namespace BenchmarkGenerate
{
    public sealed class DynamicFunction : IFunction
    {
        public int Call()
        {
            return 0;
        }
    }
}
";
            var syntaxTree = CSharpSyntaxTree.ParseText(source);
            var compilation = CSharpCompilation.Create("InMemoryAssembly")
                .WithOptions(new CSharpCompilationOptions(
                    OutputKind.DynamicallyLinkedLibrary,
                    optimizationLevel: OptimizationLevel.Release,
                    allowUnsafe: true))
                .AddReferences(MetadataReference.CreateFromFile(typeof(object).GetTypeInfo().Assembly.Location))
                .AddReferences(MetadataReference.CreateFromFile(typeof(IFunction).GetTypeInfo().Assembly.Location))
                .AddSyntaxTrees(syntaxTree);

            var stream = new MemoryStream();
            compilation.Emit(stream);

            var assembly = Assembly.Load(stream.GetBuffer());

            var type = assembly.GetType("BenchmarkGenerate.DynamicFunction");
            dynamicCompiled = (IFunction)Activator.CreateInstance(type);

        }

        [Benchmark] public int PreCompiled() => preCompiled.Call();

        [Benchmark] public int DynamicCompiled() => dynamicCompiled.Call();
    }

    public interface IFunction
    {
        int Call();
    }

    public sealed class Function : IFunction
    {
        public int Call()
        {
            return 0;
        }
    }
}
