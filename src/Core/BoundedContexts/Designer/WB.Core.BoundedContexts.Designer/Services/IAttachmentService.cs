using System;
using System.Collections.Generic;
using WB.Core.BoundedContexts.Designer.Implementation.Services.AttachmentService;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.Edit.QuestionnaireInfo;

namespace WB.Core.BoundedContexts.Designer.Services
{
    public interface IAttachmentService
    {
        void SaveAttachmentContent(Guid questionnaireId, Guid attachmentId, string attachmentContentId, AttachmentType type, string contentType, byte[] binaryContent, string fileName);
        QuestionnaireAttachment GetAttachment(Guid attachmentId);
        AttachmentContent GetAttachmentContent(string attachmentContentId);
        IEnumerable<AttachmentView> GetAttachmentsForQuestionnaire(Guid questionnaireId);
        void CloneAttachmentMeta(Guid sourceAttachmentId, Guid newAttachmentId, Guid newQuestionnaireId);
        void DeleteAttachment(Guid attachmentId);
    }
}