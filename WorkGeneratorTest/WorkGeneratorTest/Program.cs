using WorkLibrary;

namespace WorkGeneratorTest
{
    using System;

    class Program
    {
        static void Main()
        {
        }
    }

    public enum ViewId
    {
        Page1,
        Page2,
        PageA,
        PageB
    }

    [View(ViewId.Page1)]
    public class Page1
    {
    }

    [View(ViewId.Page2)]
    public class Page2
    {
    }

    [View(ViewId.PageA)]
    [View(ViewId.PageB)]
    public class PageX
    {
    }
}
