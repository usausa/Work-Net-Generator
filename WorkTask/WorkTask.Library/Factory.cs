namespace WorkTask.Library
{
    using System;

    public class Factory
    {
        private readonly Engine engine;

        public Factory(Engine engine)
        {
            this.engine = engine;
        }

        public T Create<T>()
        {
            var implType = typeof(T).Assembly.GetType(typeof(T).FullName + "_Impl");
            return (T)Activator.CreateInstance(implType, engine);
        }
    }
}
