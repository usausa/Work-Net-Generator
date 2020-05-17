namespace WorkTask.Generator.Core
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Reflection;

    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp;

    using WorkTask.Library;

    public class Generator
    {
        private static readonly HashSet<Assembly> DefaultReference = new HashSet<Assembly>();

        private readonly Action<string> logger;

        private readonly string outputFile;

        private readonly string[] sourceFiles;

        private readonly Assembly targetAssembly;

        private readonly MetadataReference targetMetadataReference;

        static Generator()
        {
            AddReference(DefaultReference, typeof(Engine).Assembly);
        }

        public Generator(string targetFile, string outputFile, string[] sourceFiles, Action<string> logger)
        {
            this.logger = logger;
            this.outputFile = outputFile;
            this.sourceFiles = sourceFiles;

            // TODO これでないとつかんでしまうが、パスロードでないと参照先を読み込めない
            var bytes = File.ReadAllBytes(targetFile);
            targetAssembly = Assembly.Load(bytes);
            targetMetadataReference = MetadataReference.CreateFromImage(bytes);
            //targetAssembly = Assembly.LoadFrom(targetFile);
            //targetMetadataReference = MetadataReference.CreateFromFile(targetFile);
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

        public bool Build()
        {
            // TODO
            //var types = targetAssembly.ExportedTypes
            //    .Where(x => x.IsInterface && (x.GetCustomAttribute<TargetAttribute>() != null))
            //    .ToArray();

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
                Path.GetFileName(outputFile),
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
                    return false;
                }

                ms.Seek(0, SeekOrigin.Begin);
                File.WriteAllBytes(outputFile, ms.ToArray());

                return true;
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
