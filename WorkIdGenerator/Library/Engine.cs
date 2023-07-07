namespace Library;

public class Engine
{
    public void Register(object id, Type type)
    {
        System.Diagnostics.Debug.WriteLine($"{id} : {type}");
    }
}
