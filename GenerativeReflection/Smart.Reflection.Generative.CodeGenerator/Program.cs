namespace Smart.Reflection.Generative.CodeGenerator
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Text;

    using Microsoft.Extensions.Configuration;

    public static class Program
    {
        public static int Main(string[] args)
        {
            if (args.Length < 1)
            {
                Console.WriteLine("Invalid arguments");
                return -1;
            }

            var generatedFile = args[0];
            var workDir = args.Length > 1 ? args[1] : Directory.GetCurrentDirectory();
            if (!Directory.Exists(workDir))
            {
                Console.WriteLine("Target directory not exists");
                return -1;
            }

            var config = new Config();
            new ConfigurationBuilder()
                .AddJsonFile(Path.Combine(workDir, "generator.json"), optional: true)
                .Build()
                .Bind(config);

            var excludes = config.ExcludeDirectories.Select(x => Path.Combine(workDir, x)).ToArray();
            var targets = config.TargetDirectories?.Select(x => Path.Combine(workDir, x)).ToArray() ?? new[] { workDir };

            var generator = new Generator();

            foreach (var file in targets.SelectMany(x => ListFiles(x, excludes, config.FilePattern)).Distinct())
            {
                var text = File.ReadAllText(file, Encoding.UTF8);
                generator.AddSource(text);
            }

            var contents = generator.Generate();

            if (File.Exists(generatedFile))
            {
                var current = File.ReadAllText(generatedFile, Encoding.UTF8);
                if (String.Equals(current, contents, StringComparison.Ordinal))
                {
                    return 0;
                }
            }

            File.WriteAllText(generatedFile, contents, Encoding.UTF8);

            return 0;
        }

        private static IEnumerable<string> ListFiles(
            string directory,
            string[] excludes,
            string filePattern)
        {
            if (!Directory.Exists(directory))
            {
                Console.WriteLine($"Directory [{directory}] is not exists");
                yield break;
            }

            foreach (var subDirectory in Directory.GetDirectories(directory))
            {
                if (excludes.Contains(subDirectory))
                {
                    continue;
                }

                foreach (var file in ListFiles(subDirectory, excludes, filePattern))
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
}
