using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;

namespace Web.Supervisor.Models
{
    using System;

    public class DocumentRequestModel
    {
        public Guid? TemplateId { get; set; }
        public Guid? ResponsibleId { get; set; }
        public InterviewStatus? Status { get; set; }
    }
}