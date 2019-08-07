namespace WorkArgument
{
    using System.Collections.Generic;
    using System.IO;
    using System.Reflection;

    class Program
    {
        static void Main(string[] args)
        {
            var lines = new List<string>();

            lines.Add(args[0]);

            lines.Add("--");
            foreach (var file in args[1].Split(";"))
            {
                if (Path.IsPathFullyQualified(file))
                {
                    lines.Add(file);
                }
                else
                {
                    lines.Add(Path.Combine(args[0], file));
                }
            }

            lines.Add("--");
            foreach (var file in args[2].Split(";"))
            {
                lines.Add(file);
            }

            File.WriteAllLines(Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "lines.txt"), lines);
            File.WriteAllLines(Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "args.txt"), args);
        }
    }
}
