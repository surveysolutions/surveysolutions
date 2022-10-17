using WB.Core.SharedKernels.Questionnaire.Documents;
using Area = WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails.Questions.Area;

namespace WB.Core.SharedKernels.Enumerator.Services.Infrastructure;

public class EditAreaArgs
{
    public EditAreaArgs(Area area, GeometryType? geometryType, int? requestedAccuracy, int? requestedFrequency,
        GeometryInputMode? requestedGeometryInputMode)
    {
        Area = area;
        GeometryType = geometryType;
        RequestedAccuracy = requestedAccuracy;
        RequestedFrequency = requestedFrequency;
        RequestedGeometryInputMode = requestedGeometryInputMode;
    }

    public Area Area { set; get; }
    public GeometryType? GeometryType { get; set; }
    public int? RequestedAccuracy { get; set; }
    public int? RequestedFrequency { get; set; }
    public GeometryInputMode? RequestedGeometryInputMode { get; set; }
    
    //neighbouring geographies here
}
