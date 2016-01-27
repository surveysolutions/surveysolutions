using System;

namespace WB.Core.SharedKernels.SurveyManagement.Views.Reposts.InputModels
{
    public class MapReportInputModel
    {
        public Guid QuestionnaireId { get; set; }
        public long QuestionnaireVersion { get; set; }
        public string Variable { get; set; }

        public double NorthEastCornerLongtitude { get; set; }
        public double NorthEastCornerLatitude { get; set; }

        public double SouthWestCornerLongtitude { get; set; }
        public double SouthWestCornerLatitude { get; set; }
    }
}