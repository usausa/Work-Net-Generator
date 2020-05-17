namespace WorkSemanticDump
{
    using System.Diagnostics;
    using System.Linq;

    using Buildalyzer;
    using Buildalyzer.Workspaces;

    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp.Syntax;

    public static class Program
    {
        public static void Main(string[] args)
        {
            var manager = new AnalyzerManager();
            var analyzer = manager.GetProject(@"..\..\..\..\WorkSemanticDump.Target\WorkSemanticDump.Target.csproj");

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

            // TODO
            Debug.WriteLine("==== Dump ====");
            var workspace = new AdhocWorkspace();
            var roslynProject = analyzer.AddToWorkspace(workspace);
            foreach (var document in roslynProject.Documents)
            {
                // Lookup interface
                var model = document.GetSemanticModelAsync().Result;
                var interfaces = model.SyntaxTree.GetRoot().DescendantNodes()
                    .OfType<InterfaceDeclarationSyntax>()
                    .ToList();
                foreach (var ifSyntax in interfaces)
                {
                    Debug.WriteLine("--------------------");
                    Debug.WriteLine(document.FilePath);

                    var ifSymbol = (INamedTypeSymbol) model.GetDeclaredSymbol(ifSyntax);
                    Debug.WriteLine($"Interface Name : {ifSymbol.ToDisplayString()}");
                    Debug.WriteLine($"Namespace : {ifSymbol.ContainingSymbol.Name}");

                    // Lookup methods
                    var methods = ifSyntax.DescendantNodes()
                        .OfType<MethodDeclarationSyntax>()
                        .ToArray();
                    foreach (var methodSyntax in methods)
                    {
                        Debug.WriteLine("----");
                        var methodSymbol = (IMethodSymbol) model.GetDeclaredSymbol(methodSyntax);

                        Debug.WriteLine($"Method Name : {methodSymbol.Name}");
                        Debug.WriteLine($"Return type : {methodSymbol.ReturnType.ToDisplayString()}");

                        // Parameters
                        foreach (var parameterSymbol in methodSymbol.Parameters)
                        {
                            Debug.WriteLine($"Parameter : {parameterSymbol.Name} {parameterSymbol.RefKind} {parameterSymbol.Type.ToDisplayString()}");
                        }
                    }
                }
            }
        }
    }
}
