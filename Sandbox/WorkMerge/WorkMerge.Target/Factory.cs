namespace WorkMerge.Target
{
    using System;

    public static class Factory
    {
        public static ITarget Create()
        {
            var type = Type.GetType("WorkMerge.Target.Option");
            var obj = Activator.CreateInstance(type);
            return (ITarget)obj;
        }
    }
}
