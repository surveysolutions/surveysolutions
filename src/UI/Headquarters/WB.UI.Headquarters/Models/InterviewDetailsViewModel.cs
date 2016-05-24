using System;

namespace WB.Core.SharedKernels.SurveyManagement.Web.Models
{
    public class InterviewDetailsViewModel
    {
        public Guid InterviewId { get; set; }
        public Guid? CurrentGroupId { get; set; }
        public Guid? CurrentPropagationKey { get; set; }
    }
}