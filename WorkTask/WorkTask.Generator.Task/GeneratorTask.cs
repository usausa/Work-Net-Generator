using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using Microsoft.Build.Framework;

namespace WorkTask.Generator.Task
{
    using Microsoft.Build.Utilities;

    public class GeneratorTask : Task
    {
        [Required]
        public ITaskItem[] SourceFiles { get; set; }

        [Required]
        public ITaskItem TargetFile { get; set; }

        [Required]
        public ITaskItem OutputFile { get; set; }

        public override bool Execute()
        {
            Log.LogMessage("Work build test.");

            Log.LogMessage("[SourceFiles]");
            foreach (var source in SourceFiles)
            {
                LogFileInfo(source.ItemSpec);
            }

            Log.LogMessage("[SourceFiles]");
            LogFileInfo(TargetFile.ItemSpec);

            Log.LogMessage("[SourceFiles]");
            LogFileInfo(OutputFile.ItemSpec);

            var assemblyBytes = File.ReadAllBytes(Path.GetFullPath(TargetFile.ItemSpec));
            var generator = new Generator.Core.Generator(assemblyBytes, message =>
            {
                Log.LogError(message);
            });

            var newBytes = generator.Build(Path.GetFileName(OutputFile.ItemSpec));
            if (newBytes == null)
            {
                return false;
            }

            File.WriteAllBytes(Path.GetFullPath(OutputFile.ItemSpec), newBytes);

            return true;
        }

        private void LogFileInfo(string file)
        {
            var path = Path.GetFullPath(file);
            Log.LogMessage(path + " " + new FileInfo(path).LastWriteTime);
        }
    }
}
