using System;

namespace WB.Core.SharedKernels.SurveyManagement.Web.Models
{
    public class InterviewsStatisticsInputModel
    {
        public Guid TemplateId { get; set; }
        public long TemplateVersion { get; set; }

        public DateTime? From { get; set; }
        public DateTime? To { get; set; }
    }
}