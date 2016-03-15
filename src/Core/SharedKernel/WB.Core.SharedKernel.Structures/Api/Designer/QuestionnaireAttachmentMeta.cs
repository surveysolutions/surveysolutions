using System;

namespace WB.Core.SharedKernels.SurveySolutions.Api.Designer
{
    public class QuestionnaireAttachmentMeta
    {
        public Guid AttachmentId { get; set; }
        public string AttachmentContentHash { get; set; }
        public string ContentType { get; set; }
    }
}