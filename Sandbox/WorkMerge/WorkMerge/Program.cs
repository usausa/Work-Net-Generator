using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using Mono.Cecil.Cil;

namespace WorkMerge
{
    using Mono.Cecil;

    class Program
    {
        private const string Target =
            "..\\..\\..\\..\\WorkMerge.Target\\bin\\Release\\netstandard2.0\\WorkMerge.Target.dll";
        private const string Option =
            "..\\..\\..\\..\\WorkMerge.Target.Option\\bin\\Release\\netstandard2.0\\WorkMerge.Target.Option.dll";

        private const string Output =
            "..\\..\\..\\..\\WorkMerge.Target\\bin\\Release\\netstandard2.0\\WorkMerge.Target2.dll";

        private static ModuleDefinition module;

        static void Main()
        {
            var target = AssemblyDefinition.ReadAssembly(Target, new ReaderParameters(ReadingMode.Immediate));
            var option = AssemblyDefinition.ReadAssembly(Option);

            var importer = new TypeImporter(target.MainModule);
            foreach (var definition in option.Modules.SelectMany(x => x.Types).ToArray())
            {
                if (!definition.FullName.StartsWith("<"))
                {
                    importer.Clone(definition);
                }
            }

            target.Write(Output);

            var assembly = Assembly.LoadFile(Path.GetFullPath(Output));
            var type = assembly.GetType("WorkMerge.Target.Option");
            var obj = Activator.CreateInstance(type, new object[] { "hoge" });
            var method = type.GetMethod("ITarget");
            var ret = method.Invoke(obj, null);
            Debug.WriteLine(ret);
        }
    }
}
