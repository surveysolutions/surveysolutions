using System;

namespace Core.Supervisor.Views.Reposts.Views
{
    public class TemplateViewItem
    {
        public Guid TemplateId { get; set; }

        public string TemplateName { get; set; }

        public long TemplateVersion { get; set; }
    }
}