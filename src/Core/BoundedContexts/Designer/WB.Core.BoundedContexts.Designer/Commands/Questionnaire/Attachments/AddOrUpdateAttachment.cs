using System;
using WB.Core.BoundedContexts.Designer.Commands.Questionnaire.Base;

namespace WB.Core.BoundedContexts.Designer.Commands.Questionnaire.Attachments
{
    [Serializable]
    public class AddOrUpdateAttachment : QuestionnaireCommand
    {
        public AddOrUpdateAttachment(
            Guid questionnaireId,
            Guid attachmentId,
            Guid responsibleId,
            string attachmentName,
            string attachmentContentId,
            Guid? oldAttachmentId)
            : base(responsibleId: responsibleId, questionnaireId: questionnaireId)
        {
            this.AttachmentId = attachmentId;
            this.AttachmentName = attachmentName;
            this.AttachmentContentId = attachmentContentId;
            this.OldAttachmentId = oldAttachmentId;
        }


        public Guid? OldAttachmentId { get; set; }
        public Guid AttachmentId { get; private set; }
        public string AttachmentContentId { get; set; }
        public string AttachmentName { get; private set; }
    }
}