namespace TargetProject.Core
{
    using TargetProject.Infrastructure;

    public interface IWorkService
    {
        [Work("Foo")]
        int Execute1(int x, int y);

        [Work("Bar")]
        int Execute2(string x, ref int a, out int b);
    }
}
