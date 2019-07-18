namespace WorkMerge
{
    using System.Diagnostics;
    using WorkMerge.Target;

    class Program
    {
        static void Main()
        {
            var obj = Factory.Create();
            Debug.WriteLine(obj.Execute());
        }
    }
}
