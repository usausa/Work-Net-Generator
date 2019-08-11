namespace WorkBuild.Target
{
    public class IExecuteDummy : IExecute
    {
        private readonly global::WorkBuild.Library.Engine engine;

        public IExecuteDummy(global::WorkBuild.Library.Engine engine)
        {
            this.engine = engine;
        }

        public global::System.Int32 Execute(global::Smart.ComponentModel.NotificationValue<global::System.Int32> value)
        {
            return this.engine.Execute<global::System.Int32>(value);
        }
    }

    public class IExecute2Dummy : IExecute2
    {
        private readonly global::WorkBuild.Library.Engine engine;

        public IExecute2Dummy(global::WorkBuild.Library.Engine engine)
        {
            this.engine = engine;
        }

        public global::System.Int32 Execute(global::System.Int32 value)
        {
            return this.engine.Execute<global::System.Int32>(value);
        }
    }
}