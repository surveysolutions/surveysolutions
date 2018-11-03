using WB.Core.SharedKernels.DataCollection.Implementation.Entities;

namespace WB.Core.BoundedContexts.Headquarters.Views.Reposts.InputModels
{
    public class MapReportInputModel
    {
        public string QuestionnaireId { get; set; }

        public QuestionnaireIdentity QuestionnaireIdentity => QuestionnaireIdentity.Parse(this.QuestionnaireId);

        public string Variable { get; set; }

        public double NorthEastCornerLongtitude { get; set; }
        public double NorthEastCornerLatitude { get; set; }

        public double SouthWestCornerLongtitude { get; set; }
        public double SouthWestCornerLatitude { get; set; }
        public int Zoom { get; set; }
    }
}
