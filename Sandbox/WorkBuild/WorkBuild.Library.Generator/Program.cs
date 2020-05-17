namespace WorkBuild.Library.Generator
{
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Runtime.Loader;

    using WorkBuild.Library.Generator.Core;

    public class Program
    {
        public static void Main(string[] args)
        {
            var path = Path.GetFullPath(args[0]);
            var outputDirectory = Path.GetFullPath(args[1]);
            var references = args[2]
                .Split(";")
                .Select(x => new Reference(x))
                .ToDictionary(x => x.Name);

            var context = AssemblyLoadContext.GetLoadContext(Assembly.LoadFile(path));
            context.Resolving += (loadContext, name) =>
            {
                if (references.TryGetValue(name.Name, out var reference))
                {
                    return context.LoadFromAssemblyPath(reference.FilePath);
                }

                return null;
            };

            var assembly = context.LoadFromAssemblyPath(path);

            var generator = new SourceGenerator
            {
                OutputDirectory = outputDirectory
            };

            generator.Generate(assembly.GetExportedTypes());
        }
    }
}
