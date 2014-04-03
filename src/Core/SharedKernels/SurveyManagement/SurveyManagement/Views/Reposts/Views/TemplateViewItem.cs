using System;

namespace WB.Core.SharedKernels.SurveyManagement.Views.Reposts.Views
{
    public class TemplateViewItem
    {
        public Guid TemplateId { get; set; }

        public string TemplateName { get; set; }

        public long TemplateVersion { get; set; }
    }
}