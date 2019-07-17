namespace WorkRoslynCodeGen.Library
{
    using System;

    public static class Hoge2Factory
    {
        public static object Create()
        {
            var type = Type.GetType("WorkRoslynCodeGen.Library.Hoge2");
            if (type is null)
            {
                return null;
            }

            return Activator.CreateInstance(type);
        }
    }
}
