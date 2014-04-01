using System;

namespace WB.Core.BoundedContexts.Headquarters.Reports.Views
{
    public class TemplateViewItem
    {
        public Guid TemplateId { get; set; }

        public string TemplateName { get; set; }

        public long TemplateVersion { get; set; }
    }
}