using System;

namespace WB.UI.Headquarters.Models
{
    public class SummaryRequestModel
    {
        public Guid? TemplateId { get; set; }

        public long? TemplateVersion { get; set; }
    }
}