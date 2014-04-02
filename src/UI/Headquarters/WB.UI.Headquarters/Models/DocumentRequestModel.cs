using System;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;

namespace WB.UI.Headquarters.Models
{
    public class DocumentRequestModel
    {
        public Guid? TemplateId { get; set; }
        public long? TemplateVersion { get; set; }
        public Guid? ResponsibleId { get; set; }
        public InterviewStatus? Status { get; set; }
    }
}