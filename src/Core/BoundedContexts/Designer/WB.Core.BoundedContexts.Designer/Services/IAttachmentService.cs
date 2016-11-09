using System;
using System.Collections.Generic;
using WB.Core.BoundedContexts.Designer.Implementation.Services.AttachmentService;

namespace WB.Core.BoundedContexts.Designer.Services
{
    public interface IAttachmentService
    {
        string CreateAttachmentContentId(byte[] binaryContent);
        void SaveContent(string contentId, string contentType, byte[] binaryContent);
        void SaveMeta(Guid attachmentId, Guid questionnaireId, string contentId, string fileName);
        AttachmentContent GetContentDetails(string contentId);
        void CloneMeta(Guid sourceAttachmentId, Guid newAttachmentId, Guid newQuestionnaireId);
        void DeleteAllByQuestionnaireId(Guid questionnaireId);
        List<AttachmentMeta> GetAttachmentsByQuestionnaire(Guid questionnaireId);
        AttachmentMeta GetAttachmentMeta(Guid attachmentId);
        AttachmentContent GetContent(string contentId);
        List<AttachmentSize> GetAttachmentSizesByQuestionnaire(Guid questionnaireId);
        string GetAttachmentContentId(Guid attachmentId);
    }
}