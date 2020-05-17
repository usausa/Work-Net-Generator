using System;
using System.Diagnostics;
using System.IO;
using Mono.Cecil;

namespace WorkAnalyze
{
    class Program
    {
        static void Main(string[] args)
        {
            var target = Path.GetFullPath(@"..\..\..\..\WorkAnalyze.Target\bin\Release\netstandard2.0\WorkAnalyze.Target.dll");
            var assembly = AssemblyDefinition.ReadAssembly(target, new ReaderParameters(ReadingMode.Immediate));

            foreach (var type in assembly.MainModule.Types)
            {
                Debug.WriteLine("--------");
                Debug.WriteLine(type.Namespace);
                Debug.WriteLine(type.Name);
                Debug.WriteLine(type.FullName);
                Debug.WriteLine(type.BaseType?.Namespace);
                Debug.WriteLine(type.BaseType?.Name);
            }
        }
    }
}
