using System.Collections.Generic;

namespace WorkTemplateLibrary
{
    public class Runner
    {
        public static string Run()
        {
            var template = new TestTemplate()
            {
                Values = new List<string>(new[] {"a", "bc", "123"})
            };
            return template.TransformText();
        }
    }
}
