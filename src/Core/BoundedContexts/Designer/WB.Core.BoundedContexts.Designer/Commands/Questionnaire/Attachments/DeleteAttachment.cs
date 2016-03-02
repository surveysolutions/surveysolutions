using System;
using WB.Core.BoundedContexts.Designer.Commands.Questionnaire.Base;

namespace WB.Core.BoundedContexts.Designer.Commands.Questionnaire.Attachments
{
    public class DeleteAttachment : QuestionnaireCommand
    {
        public DeleteAttachment(Guid questionnaireId, Guid attachmentId, Guid responsibleId)
            : base(responsibleId: responsibleId, questionnaireId: questionnaireId)
        {
            this.AttachmentId = attachmentId;
        }

        public Guid AttachmentId { get; private set; }
    }
}