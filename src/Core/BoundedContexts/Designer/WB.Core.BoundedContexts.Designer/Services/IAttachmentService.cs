using System;
using System.Collections.Generic;
using WB.Core.BoundedContexts.Designer.Implementation.Services.AttachmentService;

namespace WB.Core.BoundedContexts.Designer.Services
{
    public interface IAttachmentService
    {
        string GetAttachmentContentId(byte[] binaryContent);
        void SaveContent(string contentId, string contentType, byte[] binaryContent);
        void SaveMeta(Guid attachmentId, Guid questionnaireId, string contentId, string fileName);
        AttachmentContentView GetContentDetails(string contentId);
        void CloneMeta(Guid sourceAttachmentId, Guid newAttachmentId, Guid newQuestionnaireId);
        void Delete(Guid attachmentId);
        List<AttachmentMeta> GetAttachmentsByQuestionnaire(Guid questionnaireId);
        QuestionnaireAttachment GetAttachmentWithContent(Guid attachmentId);
        AttachmentContent GetContent(string contentId);
        List<AttachmentSize> GetAttachmentSizesByQuestionnaire(Guid questionnaireId);
    }
}