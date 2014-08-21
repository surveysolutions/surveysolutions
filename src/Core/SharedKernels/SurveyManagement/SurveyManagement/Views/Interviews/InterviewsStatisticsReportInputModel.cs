using System;

namespace WB.Core.SharedKernels.SurveyManagement.Views.Interviews
{
    public class InterviewsStatisticsReportInputModel
    {
        public DateTime CurrentDate { get; set; }
        public Guid QuestionnaireId { get; set; }
        public long? QuestionnaireVersion { get; set; }
    }
}