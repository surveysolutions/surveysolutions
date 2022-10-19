using WB.Core.SharedKernels.Questionnaire.Documents;
using Area = WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails.Questions.Area;

namespace WB.Core.SharedKernels.Enumerator.Services.Infrastructure;

public class EditAreaArgs
{
    public EditAreaArgs(Area area, GeometryType? geometryType, int? requestedAccuracy, int? requestedFrequency,
        GeometryInputMode? requestedGeometryInputMode, GeographyNeighbor[] geographyNeighbors)
    {
        Area = area;
        GeometryType = geometryType;
        RequestedAccuracy = requestedAccuracy;
        RequestedFrequency = requestedFrequency;
        RequestedGeometryInputMode = requestedGeometryInputMode;
        GeographyNeighbors = geographyNeighbors;
    }

    public Area Area { set; get; }
    public GeometryType? GeometryType { get; set; }
    public int? RequestedAccuracy { get; set; }
    public int? RequestedFrequency { get; set; }
    public GeometryInputMode? RequestedGeometryInputMode { get; set; }
    public GeographyNeighbor[] GeographyNeighbors { get; set; }
}

public class GeographyNeighbor
{
    public string Title { get; set; }
    public string Geometry { get; set; }
}
