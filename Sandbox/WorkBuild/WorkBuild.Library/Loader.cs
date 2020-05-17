using System.IO;

namespace WorkBuild.Library
{
    using System;
    using System.Reflection;

    public class Loader
    {
        private readonly Engine engine;

        public Loader(Engine engine)
        {
            this.engine = engine;
        }

        public T Load<T>()
        {
            var type = typeof(T);
            var directory = Path.GetDirectoryName(type.Assembly.Location);
            var assemblyName = $"{type.Assembly.GetName().Name}.Impl.dll";
            var assembly = Assembly.LoadFile(Path.Combine(directory, assemblyName));
            var implType = assembly.GetType($"{type.FullName}Impl");
            return (T)Activator.CreateInstance(implType, engine);
        }
    }
}
