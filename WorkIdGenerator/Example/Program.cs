namespace Example;

using Example.Modules;

using Library;

internal class Program
{
    public static void Main()
    {
        var engine = new Engine();
        engine.Register(ViewRegistry.ListViews());
    }
}

public static partial class ViewRegistry
{
    [ViewSource]
    public static partial IEnumerable<KeyValuePair<ViewId, Type>> ListViews();
}
