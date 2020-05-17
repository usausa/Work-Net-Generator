namespace Smart.Reflection.Generative
{
    using System;
    using System.Collections.Generic;
    using System.Reflection;

    /// <summary>
    ///
    /// </summary>
    public class GenerativeMetadataFactory : IActivatorFactory, IAccessorFactory
    {
        private static readonly object Sync = new object();

        private static readonly Dictionary<ConstructorInfo, IActivator> ActivatorCache = new Dictionary<ConstructorInfo, IActivator>();

        private static readonly Dictionary<PropertyInfo, IAccessor> AccessorCache = new Dictionary<PropertyInfo, IAccessor>();

        private static readonly Dictionary<PropertyInfo, IAccessor> ExtensionAccessorCache = new Dictionary<PropertyInfo, IAccessor>();

        private readonly IActivatorFactory fallbackActivatorFactory;

        private readonly IAccessorFactory fallbackAccessorFactory;

        /// <summary>
        ///
        /// </summary>
        public static GenerativeMetadataFactory Default { get; } = new GenerativeMetadataFactory();

        /// <summary>
        ///
        /// </summary>
        public GenerativeMetadataFactory()
        {
            fallbackActivatorFactory = TypeMetadataFactory.Default;
            fallbackAccessorFactory = TypeMetadataFactory.Default;
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="fallbackActivatorFactory"></param>
        /// <param name="fallbackAccessorFactory"></param>
        public GenerativeMetadataFactory(IActivatorFactory fallbackActivatorFactory, IAccessorFactory fallbackAccessorFactory)
        {
            this.fallbackActivatorFactory = fallbackActivatorFactory;
            this.fallbackAccessorFactory = fallbackAccessorFactory;
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="ci"></param>
        /// <returns></returns>
        public IActivator CreateActivator(ConstructorInfo ci)
        {
            if (ci == null)
            {
                throw new ArgumentNullException(nameof(ci));
            }

            lock (Sync)
            {
                if (!ActivatorCache.TryGetValue(ci, out var activator))
                {
                    activator = CreateActivatorInternal(ci);
                    ActivatorCache[ci] = activator;
                }

                return activator;
            }
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="ci"></param>
        /// <returns></returns>
        private IActivator CreateActivatorInternal(ConstructorInfo ci)
        {
            var holderName = $"{ci.DeclaringType.FullName}_TypeMetadataHolder";
            var holderType = Type.GetType(holderName);
            if (holderType == null)
            {
                return fallbackActivatorFactory?.CreateActivator(ci);
            }

            return (IActivator)Activator.CreateInstance(holderType);
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="pi"></param>
        /// <returns></returns>
        public IAccessor CreateAccessor(PropertyInfo pi)
        {
            return CreateAccessor(pi, true);
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="pi"></param>
        /// <param name="extension"></param>
        /// <returns></returns>
        public IAccessor CreateAccessor(PropertyInfo pi, bool extension)
        {
            if (pi == null)
            {
                throw new ArgumentNullException(nameof(pi));
            }

            lock (Sync)
            {
                var cache = extension ? ExtensionAccessorCache : AccessorCache;
                if (!cache.TryGetValue(pi, out var accessor))
                {
                    accessor = CreateAccessorInternal(pi, extension);
                    cache[pi] = accessor;
                }

                return accessor;
            }
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="pi"></param>
        /// <param name="extension"></param>
        /// <returns></returns>
        private IAccessor CreateAccessorInternal(PropertyInfo pi, bool extension)
        {
            var holderName = $"{pi.DeclaringType.FullName}_TypeMetadataHolder";
            var holderType = Type.GetType(holderName);
            if (holderType == null)
            {
                return fallbackAccessorFactory?.CreateAccessor(pi, extension);
            }

            return (IAccessor)Activator.CreateInstance(holderType);
        }
    }
}
