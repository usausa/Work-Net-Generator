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
            var path = Path.GetFullPath(@"..\..\..\..\WorkTask.Target\bin\Release\netstandard2.0\WorkTask.Target.dll");
            var bytes = File.ReadAllBytes(path);

            var generator = new Generator.Core.Generator(bytes, message =>
            {
                Debug.WriteLine(message);
            });

            var newBytes = generator.Build("WorkTask.Target2.dll");
            File.WriteAllBytes("WorkTask.Target2.dll", newBytes);

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
