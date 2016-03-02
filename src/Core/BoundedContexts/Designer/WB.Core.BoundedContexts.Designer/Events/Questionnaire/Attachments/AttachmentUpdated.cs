using System;
using Main.Core.Events.Questionnaire;

namespace WB.Core.BoundedContexts.Designer.Events.Questionnaire.Attachments
{
    public class AttachmentUpdated : QuestionnaireActiveEvent
    {
        public AttachmentUpdated() { }

        public AttachmentUpdated(Guid attachmentId, string attachmentName, string attachmentFileName, Guid responsibleId)
        {
            this.AttachmentId = attachmentId;
            this.AttachmentName = attachmentName;
            this.AttachmentFileName = attachmentFileName;
            this.ResponsibleId = responsibleId;
        }

        public Guid AttachmentId { get; set; }

        public string AttachmentName { get; set; }

        public string AttachmentFileName { get; set; }
    }
}