namespace WorkBuild.Target.Client
{
    using WorkBuild.Library;

    class Program
    {
        static void Main(string[] args)
        {
            var engine = new Engine();
            var loader = new Loader(engine);

            var impl = loader.Load<IExecute>();
        }
    }
}
