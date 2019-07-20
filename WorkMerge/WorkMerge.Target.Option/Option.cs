namespace WorkMerge.Target
{
    [Work("Option")]
    public class Option : ITarget
    {
        private readonly string message;

        public Option(string message)
        {
            this.message = message;
        }

        [Work("Execute")]
        public string Execute()
        {
            return message;
        }
    }
}
