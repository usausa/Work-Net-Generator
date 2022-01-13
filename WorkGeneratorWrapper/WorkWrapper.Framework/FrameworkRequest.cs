using System.Diagnostics.CodeAnalysis;

namespace WorkWrapper.Framework;

public class FrameworkRequest
{
    [AllowNull]
    public string Body { get; set; }
}
