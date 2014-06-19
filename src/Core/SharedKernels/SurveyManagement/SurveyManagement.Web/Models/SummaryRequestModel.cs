using System;

namespace WB.Core.SharedKernels.SurveyManagement.Web.Models
{
    public class SummaryRequestModel
    {
        public Guid? TemplateId { get; set; }

        public long? TemplateVersion { get; set; }
    }
}