using System.Collections.Generic;

namespace WB.Core.BoundedContexts.Headquarters.Repositories;

public class MapFiles
{
    public string Name { get; set; }
    public bool IsShapeFile { get; set; }
    public List<string> Files { get; set; }
}