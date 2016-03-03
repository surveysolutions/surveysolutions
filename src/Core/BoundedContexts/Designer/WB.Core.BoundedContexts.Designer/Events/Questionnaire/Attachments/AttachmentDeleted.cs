using System;
using Main.Core.Events.Questionnaire;

namespace WB.Core.BoundedContexts.Designer.Events.Questionnaire.Attachments
{
    public class AttachmentDeleted : QuestionnaireActiveEvent
    {
        public AttachmentDeleted() { }

        public AttachmentDeleted(Guid attachmentId, Guid responsibleId)
        {
            this.AttachmentId = attachmentId;
            this.ResponsibleId = responsibleId;
        }

        public Guid AttachmentId { get; set; }
    }
}