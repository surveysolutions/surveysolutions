using System;
using WB.Core.BoundedContexts.Designer.Commands.Questionnaire.Base;

namespace WB.Core.BoundedContexts.Designer.Commands.Questionnaire.Attachments
{
    [Serializable]
    public class UpdateAttachment : QuestionnaireCommand
    {
        public UpdateAttachment(
            Guid questionnaireId,
            Guid attachmentId,
            Guid responsibleId,
            string attachmentName,
            string attachmentFileName,
            string attachmentContentId)
            : base(responsibleId: responsibleId, questionnaireId: questionnaireId)
        {
            this.AttachmentId = attachmentId;
            this.AttachmentName = attachmentName;
            this.AttachmentFileName = attachmentFileName;
            this.AttachmentContentId = attachmentContentId;
        }

        public Guid AttachmentId { get; private set; }
        public string AttachmentContentId { get; set; }
        public string AttachmentName { get; private set; }
        public string AttachmentFileName { get; set; }
    }
}