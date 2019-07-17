namespace GenerateLibraryClient
{
    using System;

    using GenerateLibrary;
    using GenerateLibraryClient.Service;

    public static class Program
    {
        public static void Main(string[] args)
        {
            var builder = new Builder("InMemoryAssembly")
                .AddType(typeof(IService));
            var factory = builder.ToFactory();
            var service = factory.Create<IService>();

            Console.WriteLine(service.Method1("test", false));
            Console.WriteLine(service.Method2(null));
        }
    }

    namespace Data
    {
        public class Parameter { }

        public class Result { }
    }

    namespace Service
    {
        using GenerateLibraryClient.Data;

        public interface IService
        {
            int Method1(string param1, bool param2);

            Result Method2(Parameter parameter);
        }
    }
}
