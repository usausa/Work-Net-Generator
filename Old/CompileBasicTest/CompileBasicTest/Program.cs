namespace CompileBasicTest
{
    using System;
    using System.IO;
    using System.Reflection;
    using System.Linq;

    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp;

    public static class Program
    {
        public static void Main(string[] args)
        {
            var code = @"
public class TestClass
{
    public string CreateMessage()
    {
        return ""Hello world"";
    }
}
";
            var syntaxTree = CSharpSyntaxTree.ParseText(code);

            var references = new[]
            {
                MetadataReference.CreateFromFile(typeof(object).Assembly.Location)

            };

            var complitation = CSharpCompilation.Create(
                "GeneratedAssembly",
                new[] {syntaxTree},
                references,
                new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

            using (var ms = new MemoryStream())
            {
                var result = complitation.Emit(ms);
                if (!result.Success)
                {
                    foreach (var diagnostic in result.Diagnostics.Where(x => x.IsWarningAsError || x.Severity == DiagnosticSeverity.Error))
                    {
                        Console.WriteLine("{0}: {1}", diagnostic.Id, diagnostic.GetMessage());
                    }

                    return;
                }

                var assembly = Assembly.Load(ms.GetBuffer());

                var type = assembly.GetType("TestClass");

                var instance = Activator.CreateInstance(type);

                var message = (string)type.GetMethod("CreateMessage").Invoke(instance, null);

                Console.WriteLine(message);
            }

        }
    }
}
