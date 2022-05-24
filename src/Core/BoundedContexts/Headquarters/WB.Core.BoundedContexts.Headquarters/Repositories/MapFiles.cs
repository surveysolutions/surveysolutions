using System.Collections.Generic;

namespace WB.Core.BoundedContexts.Headquarters.Repositories;

public class MapFiles
{
    public string Name { get; set; }
    public bool IsShapeFile { get; set; }
    public List<MapFile> Files { get; set; }
    public long Size { get; set; }
}

public class MapFile
{
    public string Name { get; set; }
    public long Size { get; set; }
}