using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using WorkTask.Library;

namespace WorkTask.Work
{
    class Program
    {
        static void Main(string[] args)
        {
            var target = Path.GetFullPath("WorkTask.Target.dll");
            var output = Path.GetFullPath("WorkTask.Target_Impl.dll");
            var sources = Directory.GetFiles(@"..\..\..\..\WorkTask.Target", "*.cs").Select(Path.GetFullPath).ToArray();

            var generator = new Generator.Core.Generator(
                target,
                output,
                sources,
                message =>
                {
                    Debug.WriteLine(message);
                });

            var result = generator.Build();

            var newAssembly = Assembly.LoadFile(Path.GetFullPath("WorkTask.Target2.dll"));
            var types = newAssembly.ExportedTypes.ToArray();
            var implType = newAssembly.GetType("WorkTask.Target.IExecute_Impl");
            var instance = Activator.CreateInstance(implType, new Engine());
            var method = implType.GetMethod("Execute");
            var ret = method.Invoke(instance, null);
            Debug.WriteLine(ret);
        }
    }
}
