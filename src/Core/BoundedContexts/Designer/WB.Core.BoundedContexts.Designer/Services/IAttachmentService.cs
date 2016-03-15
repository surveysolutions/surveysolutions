using System;
using System.Collections.Generic;
using WB.Core.BoundedContexts.Designer.Implementation.Services.AttachmentService;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.Edit.QuestionnaireInfo;
using WB.Core.SharedKernels.SurveySolutions.Api.Designer;

namespace WB.Core.BoundedContexts.Designer.Services
{
    public interface IAttachmentService
    {
        void SaveAttachmentContent(Guid questionnaireId, Guid attachmentId, AttachmentType type, string contentType, byte[] binaryContent, string fileName);
        QuestionnaireAttachment GetAttachment(Guid attachmentId);
        IEnumerable<AttachmentView> GetAttachmentsForQuestionnaire(Guid questionnaireId);
        IEnumerable<QuestionnaireAttachmentMeta> GetBriefAttachmentsMetaForQuestionnaire(Guid questionnaireId);
        void CloneAttachmentMeta(Guid sourceAttachmentId, Guid newAttachmentId, Guid newQuestionnaireId);
        void DeleteAttachment(Guid attachmentId);
    }
}