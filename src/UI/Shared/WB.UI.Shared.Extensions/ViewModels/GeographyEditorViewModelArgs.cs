namespace WB.UI.Shared.Extensions.ViewModels
{
    public class GeographyEditorViewModelArgs
    {
        public string Geometry { get; set; }
        public string MapName { get; set; }
        public WB.Core.SharedKernels.Questionnaire.Documents.GeometryType? RequestedGeometryType { set; get; }
    }
}
