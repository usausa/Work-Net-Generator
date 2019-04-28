using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace GenerateLibrary
{
    using System;

    public class Builder
    {
        private readonly string assemblyName;

        private readonly List<Type> types = new List<Type>();

        public Builder(string assemblyName)
        {
            this.assemblyName = assemblyName;
        }

        public Builder AddType(Type type)
        {
            types.Add(type);
            return this;
        }

        public Factory ToFactory()
        {
            var compilation = CSharpCompilation.Create(assemblyName)
                .WithOptions(new CSharpCompilationOptions(
                    OutputKind.DynamicallyLinkedLibrary,
                    optimizationLevel: OptimizationLevel.Release,
                    allowUnsafe: true));

            var assemblies = new HashSet<Assembly>(types
                .SelectMany(GatherTypes)
                .Prepend(typeof(object))
                .Select(x => x.GetTypeInfo().Assembly));
            foreach (var assembly in assemblies)
            {
                compilation.AddReferences(MetadataReference.CreateFromFile(assembly.Location));
            }

            foreach (var type in types)
            {
                var source = GenerateSource(type);
                var syntaxTree = CSharpSyntaxTree.ParseText(source);
                compilation.AddSyntaxTrees(syntaxTree);
            }

            using (var stream = new MemoryStream())
            {
                var emitResult = compilation.Emit(stream);
                if (!emitResult.Success)
                {
                    foreach (var diagnostic in emitResult.Diagnostics.Where(x => x.IsWarningAsError || x.Severity == DiagnosticSeverity.Error))
                    {
                        Debug.WriteLine("{0}: {1}", diagnostic.Id, diagnostic.GetMessage());
                    }

                    // Exception
                    return null;
                }

                return new Factory(Assembly.Load(stream.GetBuffer()), new Engine());
            }
        }

        private IEnumerable<Type> GatherTypes(Type type)
        {
            yield return type;
            yield return type.BaseType;

            foreach (var propertyInfo in type.GetProperties())
            {
                yield return propertyInfo.PropertyType;
            }

            foreach (var methodInfo in type.GetMethods())
            {
                yield return methodInfo.ReturnType;
                foreach (var parameterInfo in methodInfo.GetParameters())
                {
                    yield return parameterInfo.ParameterType;
                }
            }

            // TODO Generic, Attribute?
        }

        private string GenerateSource(Type type)
        {
            var source = new StringBuilder();

            // TODO
            return source.ToString();
        }
    }
}
