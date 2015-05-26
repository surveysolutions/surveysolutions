using System;

namespace WB.Core.SharedKernels.SurveyManagement.Views.Interviews
{
    public class ChartStatisticsInputModel
    {
        public DateTime CurrentDate { get; set; }
        public Guid QuestionnaireId { get; set; }
        public long QuestionnaireVersion { get; set; }
        public DateTime? From { get; set; }
        public DateTime? To { get; set; }
    }
}