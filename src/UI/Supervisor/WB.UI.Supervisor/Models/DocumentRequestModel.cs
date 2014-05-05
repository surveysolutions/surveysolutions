using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;

namespace WB.UI.Supervisor.Models
{
    using System;

    public class DocumentRequestModel
    {
        public Guid? TemplateId { get; set; }
        public long? TemplateVersion { get; set; }
        public Guid? ResponsibleId { get; set; }
        public InterviewStatus? Status { get; set; }
    }
}