namespace RoslynTask
{
    using Microsoft.Build.Utilities;

    public class WorkBuildTask : Task
    {
        public override bool Execute()
        {
            Log.LogMessage("Work build test.");

            return true;
        }
    }
}
