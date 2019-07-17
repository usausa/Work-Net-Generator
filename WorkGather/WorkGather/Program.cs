namespace WorkGather
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Xml.Serialization;

    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.CSharp.Syntax;

    class Program
    {
        static void Main()
        {
            var entries = new List<InterfaceEntry>();
            foreach (var file in ListFiles("..\\..\\..\\..\\WorkGather.Target", "*.cs"))
            {
                var path = Path.GetFullPath(file);
                var source = File.ReadAllText(path);
                var tree = CSharpSyntaxTree.ParseText(source);

                entries.AddRange(tree.GetRoot().DescendantNodes()
                    .OfType<InterfaceDeclarationSyntax>()
                    .Select(iface => new InterfaceEntry
                    {
                        FullName = GetFullName(iface),
                        DocumentPath = path,
                        LastWriteTime = new FileInfo(path).LastWriteTime
                    }));
            }

            var serializer = new XmlSerializer(typeof(InterfaceEntry[]));
            using (var writer = new StreamWriter("Interfaces.xml", false, Encoding.UTF8))
            {
                serializer.Serialize(writer, entries.ToArray());
            }
        }

        private static string GetFullName(InterfaceDeclarationSyntax syntax)
        {
            var name = syntax.Identifier.Text;
            var parent = syntax.Parent;
            while (parent.IsKind(SyntaxKind.ClassDeclaration))
            {
                name = ((ClassDeclarationSyntax)parent).Identifier.Text + "+" + name;

                parent = parent.Parent;
            }

            while (parent.IsKind(SyntaxKind.NamespaceDeclaration))
            {
                name = ((NamespaceDeclarationSyntax)parent).Name + "." + name;

                parent = parent.Parent;
            }

            return name;
        }

        private static IEnumerable<string> ListFiles(string directory, string filePattern)
        {
            foreach (var subDirectory in Directory.GetDirectories(directory))
            {
                foreach (var file in ListFiles(subDirectory, filePattern))
                {
                    yield return file;
                }
            }

            foreach (var file in Directory.GetFiles(directory, filePattern))
            {
                yield return file;
            }
        }
    }

    public class InterfaceEntry
    {
        public string FullName { get; set; }

        public string DocumentPath { get; set; }

        public DateTime LastWriteTime { get; set; }
    }
}
