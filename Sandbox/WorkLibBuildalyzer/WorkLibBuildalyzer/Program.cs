namespace WorkLibBuildalyzer
{
    using System.Diagnostics;
    using System.Linq;

    using Buildalyzer;
    using Buildalyzer.Workspaces;

    using Microsoft.CodeAnalysis;

    public static class Program
    {
        public static void Main(string[] args)
        {
            var manager = new AnalyzerManager();
            var analyzer = manager.GetProject(@"..\..\..\..\TargetProject.Core\TargetProject.Core.csproj");

            var watch = Stopwatch.StartNew();
            var results = analyzer.Build();
            var elapsed = watch.ElapsedMilliseconds;

            Debug.WriteLine($"Build. {elapsed}ms");

            var result = results.First();

            Debug.WriteLine("==== Source ====");
            foreach (var source in result.SourceFiles)
            {
                Debug.WriteLine(source);
            }

            Debug.WriteLine("==== Reference ====");
            foreach (var reference in result.References)
            {
                Debug.WriteLine(reference);
            }

            Debug.WriteLine("==== ProjectReference ====");
            foreach (var project in result.ProjectReferences)
            {
                Debug.WriteLine(project);
            }

            //Debug.WriteLine("==== Property ====");
            //foreach (var property in result.Properties)
            //{
            //    Debug.WriteLine($"{property.Key} : {property.Value}");
            //}

            // To Roslyn
            Debug.WriteLine("==== Roslyn ====");
            var workspace = new AdhocWorkspace();
            var roslynProject = analyzer.AddToWorkspace(workspace);
            foreach (var document in roslynProject.Documents)
            {
                Debug.WriteLine(document.FilePath);
            }
        }
    }
}
