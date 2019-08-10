namespace WorkContext
{
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Runtime.Loader;

    public class Reference
    {
        public string Name { get; }

        public string FilePath { get; }

        public Reference(string filePath)
        {
            Name = Path.GetFileNameWithoutExtension(filePath);
            FilePath = filePath;
        }
    }

    class Program
    {
        static void Main()
        {
            var file = @"..\..\..\..\WorkContext.Library\bin\Debug\netstandard2.0\Reference.txt";
            var lib = @"..\..\..\..\WorkContext.Library\bin\Debug\netstandard2.0\WorkContext.Library.dll";

            var references = File.ReadAllLines(file)
                .Select(x => new Reference(x))
                .ToDictionary(x => x.Name);

            var path = Path.GetFullPath(lib);
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

            foreach (var type in assembly.GetExportedTypes())
            {
                Debug.WriteLine("----------");
                Debug.WriteLine(type.FullName);
                foreach (var method in type.GetMethods())
                {
                    Debug.WriteLine(method.Name);
                    foreach (var parameter in method.GetParameters())
                    {
                        Debug.WriteLine(parameter.ParameterType);
                    }
                }
            }
        }
    }
}
