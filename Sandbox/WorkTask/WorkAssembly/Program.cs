using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using WorkAssembly.Library;

namespace WorkAssembly
{
    class Program
    {
        static void Main(string[] args)
        {
            var target = @"..\..\..\..\WorkAssembly.Target\bin\Release\netstandard2.0\WorkAssembly.Target.dll";

            var assembly = Assembly.LoadFile(Path.GetFullPath(target));
            //var assembly = Assembly.Load(File.ReadAllBytes(Path.GetFullPath(target)));

            foreach (var type in assembly.ExportedTypes)
            {
                Console.WriteLine(type.FullName);
                Console.WriteLine(type.BaseType.FullName);
                Console.WriteLine(type.GetCustomAttribute<WorkAttribute>() != null);
            }
        }
    }
}
