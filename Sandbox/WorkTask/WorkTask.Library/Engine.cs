namespace WorkTask.Library
{
    using System.Reflection;

    public class Engine
    {
        public int Execute(MethodInfo mi)
        {
            return mi.Name.Length;
        }
    }
}
