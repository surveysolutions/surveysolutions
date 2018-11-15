namespace WB.UI.Shared.Extensions.CustomServices.AreaEditor
{
    public class AreaEditorViewModelArgs
    {
        public string Geometry { get; set; }
        public string MapName { get; set; }
        public WB.Core.SharedKernels.Questionnaire.Documents.GeometryType? RequestedGeometryType { set; get; }
    }
}