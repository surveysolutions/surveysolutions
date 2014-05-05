namespace Web.Supervisor.Models
{
    using System;

    public class SummaryRequestModel
    {
        public Guid? TemplateId { get; set; }

        public long? TemplateVersion { get; set; }
    }
}