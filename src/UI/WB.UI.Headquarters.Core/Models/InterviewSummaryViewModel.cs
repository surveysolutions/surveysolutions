using System;

namespace WB.Core.SharedKernels.SurveyManagement.Web.Models
{
    public class InterviewSummaryForMapPointViewModel
    {
        public Guid InterviewId { get; set; }
    }

    public class InterviewSummaryForMapPointsViewModel
    {
        public Guid[] InterviewIds { get; set; }
    }
}
