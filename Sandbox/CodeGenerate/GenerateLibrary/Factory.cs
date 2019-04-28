namespace GenerateLibrary
{
    using System;
    using System.Reflection;

    public class Factory
    {
        private readonly Assembly assembly;

        private readonly Engine engine;

        public Factory(Assembly assembly, Engine engine)
        {
            this.assembly = assembly;
            this.engine = engine;
        }

        public T Create<T>()
        {
            var ifType = typeof(T);
            var name = ifType.FullName + "Impl";
            var implType = assembly.GetType(name);
            return (T)Activator.CreateInstance(implType, engine);
        }
    }
}
