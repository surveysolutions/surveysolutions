using System.Linq;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.SharedKernels.SurveyManagement.Services;
using WB.Core.SharedKernels.SurveyManagement.Views.Questionnaire;

namespace WB.Core.SharedKernels.SurveyManagement.Implementation.Services
{
    public class QuestionnaireAttachmentService : IQuestionnaireAttachmentService
    {
        private readonly IPlainStorageAccessor<QuestionnaireAttachment> attachmentStorage;

        public QuestionnaireAttachmentService(IPlainStorageAccessor<QuestionnaireAttachment> attachmentStorage)
        {
            this.attachmentStorage = attachmentStorage;
        }

        public void SaveAttachment(string attachmentHash, string contentType, byte[] content)
        {
            this.attachmentStorage.Store(new QuestionnaireAttachment
            {
                AttachmentHash = attachmentHash,
                ContentType = contentType,
                Content = content
            }, attachmentHash);
        }

        public void DeleteAttachment(string attachmentHash)
        {
            this.attachmentStorage.Remove(attachmentHash);
        }

        public bool HasAttachment(string attachmentHash)
        {
            return this.attachmentStorage.Query(attachments => attachments.Any(attachment => attachment.AttachmentHash == attachmentHash));
        }

        public byte[] GetAttachment(string attachmentHash)
        {
            return this.attachmentStorage.GetById(attachmentHash)?.Content;
        }
    }
}
