namespace WB.UI.Supervisor.Models
{
    using System;

    public class SummaryRequestModel
    {
        public Guid? TemplateId { get; set; }

        public long? TemplateVersion { get; set; }
    }
}