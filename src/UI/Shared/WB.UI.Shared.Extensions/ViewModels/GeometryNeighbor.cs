using Esri.ArcGISRuntime.Data;
using Esri.ArcGISRuntime.Geometry;

namespace WB.UI.Shared.Extensions.ViewModels;

public class GeometryNeighbor
{
    public string Id { get; set; }
    public string Title { get; set; }
    public Geometry Geometry { get; set; }
    public bool IsOverlapping { get; set; }
    public Feature Feature { get; set; }
}