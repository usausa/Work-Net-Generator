namespace GenerateLibrary
{
    using System;

    public class Engine
    {
        public T Execute<T>(MethodMetadata md, params object[] parameters)
        {
            Console.WriteLine(md.Name);
            for (var i = 0; i < parameters.Length; i++)
            {
                var parameter = parameters[i];
                Console.WriteLine(parameter == null ? $"  {i}: null" : $"  {i}: type={parameter.GetType()}, value={parameter}");
            }

            return Activator.CreateInstance<T>();
        }
    }
}
