namespace WorkInlineIL
{
    using System;
    using System.Linq.Expressions;
    using System.Reflection;
    using System.Runtime.CompilerServices;

    using BenchmarkDotNet.Attributes;
    using BenchmarkDotNet.Configs;
    using BenchmarkDotNet.Diagnosers;
    using BenchmarkDotNet.Exporters;
    using BenchmarkDotNet.Jobs;
    using BenchmarkDotNet.Running;

    using InlineIL;
    using static InlineIL.IL.Emit;

    public static class Program
    {
        public static void Main()
        {
            BenchmarkRunner.Run<Benchmark>();
        }
    }

    public class BenchmarkConfig : ManualConfig
    {
        public BenchmarkConfig()
        {
            AddExporter(MarkdownExporter.Default, MarkdownExporter.GitHub);
            AddDiagnoser(MemoryDiagnoser.Default);
            AddJob(Job.MediumRun);
        }
    }


    [Config(typeof(BenchmarkConfig))]
    public class Benchmark
    {
        private const int N = 1000;

        private readonly Factory factory = new Factory();

        private Func<IContext, object> directDelegate;
        private Func<IContext, object> compiledDelegate;

        private IntPtr pointer;

        [GlobalSetup]
        public void Setup()
        {
            directDelegate = c => new object();
            compiledDelegate = ExpressionCompiler.Compile(c => new object());

            pointer = FactoryHelper.GetPointer();
        }

        [Benchmark(OperationsPerInvoke = N)]
        public object DelegateDirect()
        {
            object ret = null;
            for (var i = 0; i < N; i++)
            {
                ret = directDelegate(null);
            }
            return ret;
        }

        [Benchmark(OperationsPerInvoke = N)]
        public object DelegateCompiled()
        {
            object ret = null;
            for (var i = 0; i < N; i++)
            {
                ret = compiledDelegate(null);
            }
            return ret;
        }

        [Benchmark(OperationsPerInvoke = N)]
        public object Pointer()
        {
            object ret = null;
            for (var i = 0; i < N; i++)
            {
                ret = FactoryHelper.CallMethod(factory, pointer, null);
            }
            return ret;
        }
    }

    public interface IContext
    {
    }

    public class Factory
    {
        public object Create(IContext context) => new object();
    }

    // TODO Static ?
    public static class FactoryHelper
    {
        public static IntPtr GetPointer()
        {
            Ldftn(new MethodRef(typeof(Factory), nameof(Factory.Create), new TypeRef(typeof(IContext))));
            return IL.Return<IntPtr>();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static object CallMethod(Factory instance, IntPtr pointer, IContext context)
        {
            IL.Push(instance);
            IL.Push(context);
            IL.Push(pointer);
            Calli(new StandAloneMethodSig(CallingConventions.Standard | CallingConventions.HasThis, typeof(object), typeof(IContext)));
            return IL.Return<object>();
        }
    }

    public static class ExpressionCompiler
    {
        public static Func<IContext, object> Compile(Expression<Func<IContext, object>> expression)
        {
            return expression.Compile();
        }
    }

}
