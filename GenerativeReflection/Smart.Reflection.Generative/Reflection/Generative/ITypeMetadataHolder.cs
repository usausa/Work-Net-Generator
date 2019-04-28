namespace Smart.Reflection.Generative
{
    using System.Reflection;

    /// <summary>
    ///
    /// </summary>
    public interface ITypeMetadataHolder
    {
        /// <summary>
        ///
        /// </summary>
        /// <param name="ci"></param>
        /// <returns></returns>
        IActivator FindActivator(ConstructorInfo ci);

        /// <summary>
        ///
        /// </summary>
        /// <param name="pi"></param>
        /// <returns></returns>
        IAccessor FindAccessor(PropertyInfo pi);

        /// <summary>
        ///
        /// </summary>
        /// <param name="pi"></param>
        /// <param name="extension"></param>
        /// <returns></returns>
        IAccessor FindAccessor(PropertyInfo pi, bool extension);
    }
}
