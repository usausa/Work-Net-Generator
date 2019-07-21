using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using WorkTask.Library;

namespace WorkTask.Generator.Core
{
    public class Generator
    {
        private static readonly HashSet<Assembly> DefaultReference = new HashSet<Assembly>();

        private readonly Action<string> logger;

        private readonly Assembly targetAssembly;

        private readonly MetadataReference targetMetadataReference;

        static Generator()
        {
            AddReference(DefaultReference, typeof(Engine).Assembly);
        }

        public Generator(byte[] assemblyBytes, Action<string> logger)
        {
            this.logger = logger;
            targetAssembly = Assembly.Load(assemblyBytes);
            targetMetadataReference = MetadataReference.CreateFromImage(assemblyBytes);
        }

        private static void AddReference(HashSet<Assembly> assemblies, Assembly assembly)
        {
            if (assemblies.Contains(assembly))
            {
                return;
            }

            assemblies.Add(assembly);

            foreach (var assemblyName in assembly.GetReferencedAssemblies())
            {
                AddReference(assemblies, Assembly.Load(assemblyName));
            }
        }

        public byte[] Build(string name)
        {
            var source = CreateSource();
            var syntax = CSharpSyntaxTree.ParseText(source);

            var references = new HashSet<Assembly>(DefaultReference);

            var metadataReferences = references
                .Select(x => MetadataReference.CreateFromFile(x.Location))
                .Append(targetMetadataReference)
                .ToArray();

            var options = new CSharpCompilationOptions(
                OutputKind.DynamicallyLinkedLibrary,
                optimizationLevel: OptimizationLevel.Release);

            var compilation = CSharpCompilation.Create(
                name,
                new[] { syntax },
                metadataReferences,
                options);

            using (var ms = new MemoryStream())
            {
                var result = compilation.Emit(ms);

                if (!result.Success)
                {
                    foreach (var diagnostic in result.Diagnostics)
                    {
                        logger?.Invoke(diagnostic.GetMessage());
                    }
                    return null;
                }

                ms.Seek(0, SeekOrigin.Begin);
                return ms.ToArray();
            }
        }

        private string CreateSource()
        {
            return @"
namespace WorkTask.Target
{
    using WorkTask.Library;

    public sealed class IExecute_Impl : IExecute
    {
        private readonly Engine engine;

        public IExecute_Impl(Engine engine)
        {
            this.engine = engine;
        }

        public int Execute()
        {
            return engine.Execute(GetType().GetMethod(nameof(Execute)));
        }
    }
}
";
        }
    }
}
