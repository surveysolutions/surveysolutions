using System;

namespace WB.Core.BoundedContexts.Designer.ImportExport.Models;

public class CriticalityCondition
{
    public Guid? Id { get; set; }
    public string? Message { get; set; }
    public string? Expression { get; set; }
    public string? Description { get; set; }
}
