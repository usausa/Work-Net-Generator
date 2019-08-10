using System.Text;

namespace WorkBuild.Library.Generator.Core
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Reflection;

    public class SourceGenerator
    {
        public string OutputDirectory { get; set; }

        public void Generate(Type[] types)
        {
            var targetTypes = types.Where(x => x.GetCustomAttribute<ExecuteAttribute>() != null).ToArray();

            if (!Directory.Exists(OutputDirectory))
            {
                Directory.CreateDirectory(OutputDirectory);
            }

            var newFiles = new List<string>();
            foreach (var targetType in targetTypes)
            {
                var filename = targetType.FullName.Replace('.', '_').Replace('+', '_') + ".cs";
                var path = Path.Combine(OutputDirectory, filename);

                newFiles.Add(filename);

                var source = CreateSource(targetType);

                if (File.Exists(path))
                {
                    var currentSource = File.ReadAllText(path);
                    if (currentSource == source)
                    {
                        continue;
                    }
                }

                File.WriteAllText(path, source);
            }

            var currentFiles = Directory.GetFiles(OutputDirectory).Select(Path.GetFileName);
            foreach (var file in currentFiles.Except(newFiles))
            {
                File.Delete(Path.Combine(OutputDirectory, file));
            }
        }

        private string CreateSource(Type type)
        {
            var source = new StringBuilder();
            source.AppendLine($"namespace {type.Namespace}");
            source.AppendLine("{");

            // TODO using

            source.AppendLine($"    public class {type.Name}Impl : {type.Name}");
            source.AppendLine("    {");

            source.AppendLine("        private readonly global::WorkBuild.Library.Engine engine;");
            source.AppendLine("");
            source.AppendLine($"        public {type.Name}Impl(global::WorkBuild.Library.Engine engine)");
            source.AppendLine("        {");
            source.AppendLine("            this.engine = engine;");
            source.AppendLine("        }");

            // Method
            foreach (var method in type.GetMethods())
            {
                source.AppendLine("");
                source.Append("        public ");
                MakeGlobalName(source, method.ReturnType);
                source.Append($" {method.Name}(");

                foreach (var parameter in method.GetParameters())
                {
                    MakeGlobalName(source, parameter.ParameterType);
                    source.Append($" {parameter.Name}");
                    source.Append(", ");
                }

                if (method.GetParameters().Length > 0)
                {
                    source.Length -= 2;
                }

                source.AppendLine(")");
                source.AppendLine("        {");

                source.Append("            return this.engine.Execute<");
                MakeGlobalName(source, method.ReturnType);
                source.Append(">(");

                foreach (var parameter in method.GetParameters())
                {
                    source.Append($"{parameter.Name}");
                    source.Append(", ");
                }

                if (method.GetParameters().Length > 0)
                {
                    source.Length -= 2;
                }

                source.AppendLine(");");

                source.AppendLine("        }");
            }

            source.AppendLine("    }");

            source.AppendLine("}");

            return source.ToString();
        }

        public static void MakeGlobalName(StringBuilder sb, Type type)
        {
            if (type == typeof(void))
            {
                sb.Append("void");
                return;
            }

            if (type.IsGenericType)
            {
                var index = type.FullName.IndexOf('`');
                sb.Append("global::").Append(type.FullName.Substring(0, index).Replace('+', '.'));
                sb.Append("<");

                var first = true;
                foreach (var argumentType in type.GetGenericArguments())
                {
                    if (first)
                    {
                        first = false;
                    }
                    else
                    {
                        sb.Append(", ");
                    }

                    MakeGlobalName(sb, argumentType);
                }

                sb.Append(">");
            }
            else
            {
                sb.Append("global::").Append(type.FullName.Replace('+', '.'));
            }
        }
    }
}
