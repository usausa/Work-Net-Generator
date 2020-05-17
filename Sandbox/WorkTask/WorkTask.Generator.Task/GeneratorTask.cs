namespace WorkTask.Generator.Task
{
    using System.IO;
    using System.Linq;

    using Microsoft.Build.Framework;
    using Microsoft.Build.Utilities;

    public class GeneratorTask : Task
    {
        [Required]
        public ITaskItem[] SourceFiles { get; set; }

        [Required]
        public ITaskItem[] ReferenceFiles { get; set; }

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

            Log.LogMessage("[ReferenceFiles]");
            foreach (var reference in ReferenceFiles)
            {
                LogFileInfo(reference.ItemSpec);
            }

            Log.LogMessage("[SourceFiles]");
            LogFileInfo(TargetFile.ItemSpec);

            Log.LogMessage("[SourceFiles]");
            LogFileInfo(OutputFile.ItemSpec);

            var generator = new Generator.Core.Generator(
                Path.GetFullPath(TargetFile.ItemSpec),
                Path.GetFullPath(OutputFile.ItemSpec),
                SourceFiles.Select(x => Path.GetFullPath(x.ItemSpec)).ToArray(),
                message =>
                {
                    Log.LogError(message);
                });

            return generator.Build();
        }

        private void LogFileInfo(string file)
        {
            var path = Path.GetFullPath(file);
            Log.LogMessage(path + " " + new FileInfo(path).LastWriteTime);
        }
    }
}
