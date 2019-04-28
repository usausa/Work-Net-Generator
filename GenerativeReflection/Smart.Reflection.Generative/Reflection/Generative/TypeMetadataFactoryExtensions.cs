namespace Smart.Reflection.Generative
{
    public static class TypeMetadataFactoryExtensions
    {
        public static void ConfigureGenerative(this TypeMetadataFactory factory)
        {
            var generativeMetadataFactory = new GenerativeMetadataFactory(factory, factory);
            factory.ActivatorFactory = generativeMetadataFactory;
            factory.AccessorFactory = generativeMetadataFactory;
        }

        public static void ConfigureGenerative(
            this TypeMetadataFactory factory,
            IActivatorFactory fallbackActivatorFactory,
            IAccessorFactory fallbackAccessorFactory)
        {
            var generativeMetadataFactory = new GenerativeMetadataFactory(fallbackActivatorFactory, fallbackAccessorFactory);
            factory.ActivatorFactory = generativeMetadataFactory;
            factory.AccessorFactory = generativeMetadataFactory;
        }
    }
}
