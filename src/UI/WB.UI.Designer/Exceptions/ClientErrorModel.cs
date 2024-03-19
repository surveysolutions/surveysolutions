using System.Collections.Generic;

namespace WB.UI.Designer.Exceptions;

public class ClientErrorModel
{
    public string? Message { get; set; }
    public Dictionary<string, string> AdditionalData { get; set; } = new();
}
