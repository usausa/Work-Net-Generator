namespace WorkTask.Library
{
    using System;

    [AttributeUsage(AttributeTargets.Interface)]
    public sealed class TargetAttribute : Attribute
    {
        public string Name { get; }

        public TargetAttribute(string name)
        {
            Name = name;
        }
    }
}
