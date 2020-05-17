namespace LearnPropertyMetadata
{
    using System;
    using System.Diagnostics;
    using System.Linq;
    using System.Reflection;

    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.CSharp.Syntax;

    using Smart.ComponentModel;

    public static class Program
    {
        public static void Main()
        {
            var tree = CSharpSyntaxTree.ParseText(@"
using Smart.ComponentModel;

namespace Test
{
    public class Data
    {
        public int Member1 { get; set; }

        public string Member2 { get; set; }

        public int? Member3 { get; set; }

        public NotificationValue<int> Member4 { get; set; }
    }
}");
            var compilation = CSharpCompilation.Create("Work", syntaxTrees: new[] { tree })
                .AddReferences(
                    MetadataReference.CreateFromFile(typeof(Object).GetTypeInfo().Assembly.Location),
                    MetadataReference.CreateFromFile(typeof(IValueHolder<>).GetTypeInfo().Assembly.Location));
            var model = compilation.GetSemanticModel(tree);

            foreach (var syntax in tree.GetRoot().DescendantNodes().OfType<PropertyDeclarationSyntax>())
            {
                var symbol = model.GetDeclaredSymbol(syntax);

                Debug.WriteLine("--");
                Debug.WriteLine(symbol.ToString());
                Debug.WriteLine(symbol.ContainingSymbol);
                Debug.WriteLine(symbol.Type);
            }
        }
    }
}