namespace WorkTask.Target.Client
{
    using System.Diagnostics;

    using WorkTask.Library;

    public static class Program
    {
        public static void Main(string[] args)
        {
            var factory = new Factory(new Engine());
            var execute = factory.Create<IExecute>();
            Debug.WriteLine(execute.Execute());
        }
    }
}
