namespace Smart.Reflection.Generative
{
    using System.Reflection;

    public interface IGenerativeFactory
    {
        IActivator CreateActivator(ConstructorInfo ci);

        IAccessor CreateAccseor(PropertyInfo pi);
    }
}
