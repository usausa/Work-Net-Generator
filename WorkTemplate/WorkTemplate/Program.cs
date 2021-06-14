using System.Diagnostics;

namespace WorkTemplate
{
    using WorkTemplateLibrary;

    class Program
    {
        static void Main()
        {
            var str = Runner.Run();
            Debug.WriteLine(str);
        }
    }
}
