using WB.Core.SharedKernels.Questionnaire.Documents;
using System.Collections.Generic;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure;

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
        public GeographyNeighbor[] GeographyNeighbors { get; set; }
    }
}
