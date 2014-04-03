using System;

namespace WB.Core.SharedKernels.SurveyManagement.Views.Reposts.InputModels
{
    public class MapReportInputModel
    {
        public Guid QuestionnaireId { get; set; }
        public long QuestionnaireVersion { get; set; }
        public string Variable { get; set; }
    }
}