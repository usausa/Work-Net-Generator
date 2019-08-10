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
            var assemblyName = $"{type.Assembly.GetName().Name}.Impl";
            var assembly = Assembly.Load(assemblyName);
            var implType = assembly.GetType($"{type.FullName}Impl");
            return (T)Activator.CreateInstance(implType, engine);
        }
    }
}
