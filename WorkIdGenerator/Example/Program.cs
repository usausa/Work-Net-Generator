namespace Example;

using Example.Modules;

using Library;

internal class Program
{
    public static void Main()
    {
        var engine = new Engine();
        ViewRegistry.AddViews((v, t) => engine.Register(v, t));
    }
}

public static partial class ViewRegistry
{
    [ViewRegistration]
    public static partial void AddViews(Action<ViewId, Type> action);
}
