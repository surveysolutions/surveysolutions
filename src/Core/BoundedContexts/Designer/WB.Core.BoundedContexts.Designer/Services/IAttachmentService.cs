using System;
using System.Collections.Generic;
using WB.Core.BoundedContexts.Designer.Implementation.Services.AttachmentService;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.Edit.QuestionnaireInfo;

namespace WB.Core.BoundedContexts.Designer.Services
{
    public interface IAttachmentService
    {
        void SaveAttachmentContent(Guid questionnaireId, Guid attachmentId, AttachmentType type, string contentType, byte[] binaryContent, string fileName);
        QuestionnaireAttachment GetAttachment(Guid attachmentId);
        IEnumerable<AttachmentView> GetAttachmentsForQuestionnaire(string questionnaireId);
        void CloneAttachmentMeta(Guid sourceAttachmentId);
        void UpdateAttachmentName(Guid questionnaireId, Guid attachmentId, string name);
        void DeleteAttachment(Guid attachmentId);
    }
}