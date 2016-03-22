using System;
using Main.Core.Events.Questionnaire;

namespace WB.Core.BoundedContexts.Designer.Events.Questionnaire.Attachments
{
    public class AttachmentUpdated : QuestionnaireActiveEvent
    {
        public AttachmentUpdated() { }

        public AttachmentUpdated(Guid attachmentId, string attachmentContentId, string attachmentName, Guid responsibleId)
        {
            this.AttachmentId = attachmentId;
            this.AttachmentContentId = attachmentContentId;
            this.AttachmentName = attachmentName;
            this.ResponsibleId = responsibleId;
        }

        public string AttachmentContentId { get; set; }

        public Guid AttachmentId { get; set; }

        public string AttachmentName { get; set; }
    }
}