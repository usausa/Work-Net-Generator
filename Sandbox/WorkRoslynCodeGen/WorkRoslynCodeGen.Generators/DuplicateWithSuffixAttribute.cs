namespace WorkRoslynCodeGen.Generators
{
    using System;
    using System.Diagnostics;
    using CodeGeneration.Roslyn;

    [AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = true)]
    [CodeGenerationAttribute(typeof(DuplicateWithSuffixGenerator))]
    [Conditional("CodeGeneration")]
    public class DuplicateWithSuffixAttribute : Attribute
    {
        public string Suffix { get; }

        public DuplicateWithSuffixAttribute(string suffix)
        {
            Suffix = suffix;
        }
    }
}
