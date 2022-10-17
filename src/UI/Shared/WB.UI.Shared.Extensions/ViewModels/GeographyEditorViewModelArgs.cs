namespace WB.UI.Shared.Extensions.ViewModels
{
    public class GeographyEditorViewModelArgs
    {
        public string Geometry { get; set; }
        public string MapName { get; set; }
        public Core.SharedKernels.Questionnaire.Documents.GeometryType? RequestedGeometryType { set; get; }

        public int? RequestedAccuracy { set; get; }

        public int? RequestedFrequency { set; get; }
        
        public Core.SharedKernels.Questionnaire.Documents.GeometryInputMode? RequestedGeometryInputMode { set; get; }
    }
}
