namespace WorkTask.Target
{
    using WorkTask.Library;

    [Target("Work")]
    public interface IExecute
    {
        int Execute();
    }
}
