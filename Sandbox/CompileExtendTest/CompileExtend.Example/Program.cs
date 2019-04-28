namespace CompileExtend.Example
{
    using System;

    using CompileExtend.Library;

    public static class Program
    {
        public static void Main(string[] args)
        {
            var code = "Console.WriteLine(\"Hello world\");";

            var executor = Builder.Build(
                "TestExecutor",
                "",
                code,
                new[] { typeof(Console) });

            executor.Execute();
        }
    }
}
