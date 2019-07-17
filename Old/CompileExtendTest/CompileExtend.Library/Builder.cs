namespace CompileExtend.Library
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Reflection;

    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp;

    public static class Builder
    {
        public static IExecutor Build(string name, string usings, string code, Type[] types)
        {
            var source = @"
namespace CompileExtend.Generate
{
    using System;
    using CompileExtend.Library;
    " + usings + @"

    public sealed class " + name + @" : IExecutor
    {
        public void Execute()
        {
            " + code + @"
        }
    }
}";

            var syntaxTree = CSharpSyntaxTree.ParseText(source);

            var references = new List<MetadataReference>
            {
                MetadataReference.CreateFromFile(typeof(object).Assembly.Location),
                MetadataReference.CreateFromFile(typeof(IExecutor).Assembly.Location)
            };

            // TODO
            var path = Path.GetDirectoryName(typeof(object).Assembly.Location);
            references.Add(MetadataReference.CreateFromFile(Path.Combine(path, "mscorlib.dll")));
            references.Add(MetadataReference.CreateFromFile(Path.Combine(path, "System.dll")));
            references.Add(MetadataReference.CreateFromFile(Path.Combine(path, "System.Core.dll")));
            references.Add(MetadataReference.CreateFromFile(Path.Combine(path, "System.Runtime.dll")));

            references.AddRange(types.Select(x => MetadataReference.CreateFromFile(x.Assembly.Location)));

            var complitation = CSharpCompilation.Create(
                "GeneratedAssembly",
                new[] { syntaxTree },
                references,
                new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

            using (var ms = new MemoryStream())
            {
                var result = complitation.Emit(ms);
                if (!result.Success)
                {
                    return null;
                }

                var assembly = Assembly.Load(ms.GetBuffer());
                var type = assembly.GetType("CompileExtend.Generate." + name);
                return (IExecutor)Activator.CreateInstance(type);
            }
        }
    }
}
