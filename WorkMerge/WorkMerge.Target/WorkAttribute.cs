namespace WorkMerge.Target
{
    using System;

    public sealed class WorkAttribute : Attribute
    {
        public string Name { get; }

        public WorkAttribute(string name)
        {
            Name = name;
        }
    }
}
