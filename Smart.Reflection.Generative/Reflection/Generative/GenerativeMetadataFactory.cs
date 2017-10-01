namespace Smart.Reflection.Generative
{
    using System.Reflection;

    public class GenerativeMetadataFactory : IActivatorFactory, IAccessorFactory
    {
        // TODO Fallback?

        public IActivator CreateActivator(ConstructorInfo ci)
        {
            throw new System.NotImplementedException();
        }

        public IAccessor CreateAccessor(PropertyInfo pi)
        {
            throw new System.NotImplementedException();
        }

        public IAccessor CreateAccessor(PropertyInfo pi, bool extension)
        {
            throw new System.NotImplementedException();
        }
    }
}
