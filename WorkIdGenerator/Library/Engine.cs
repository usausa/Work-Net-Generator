namespace Library;

public class Engine
{
    public void Register(object id, Type type)
    {
        System.Diagnostics.Debug.WriteLine($"{id} : {type}");
    }
}

public static class EngineExtensions
{
    public static void Register<T>(this Engine engine, IEnumerable<KeyValuePair<T, Type>> source)
    {
        foreach (var pair in source)
        {
            engine.Register(pair.Key!, pair.Value);
        }
    }
}
