using WB.Core.SharedKernels.Questionnaire.Documents;

namespace WB.UI.Shared.Extensions.ViewModels
{
    public class GeographyEditorViewModelArgs
    {
        public string Geometry { get; set; }
        public string MapName { get; set; }
        public GeometryType? RequestedGeometryType { set; get; }

        public int? RequestedAccuracy { set; get; }

        public int? RequestedFrequency { set; get; }
        
        public GeometryInputMode? RequestedGeometryInputMode { set; get; }
    }
}
