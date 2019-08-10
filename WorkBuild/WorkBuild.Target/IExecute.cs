namespace WorkBuild.Target
{
    using Smart.ComponentModel;

    using WorkBuild.Library;

    [Execute]
    public interface IExecute
    {
        int Execute(NotificationValue<int> value);
    }
}
