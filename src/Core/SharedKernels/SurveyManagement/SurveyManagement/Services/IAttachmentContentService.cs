using WB.Core.SharedKernels.SurveyManagement.Views.Questionnaire;

namespace WB.Core.SharedKernels.SurveyManagement.Services
{
    public interface IAttachmentContentService
    {
        void SaveAttachmentContent(string contentHash, string contentType, byte[] content);
        AttachmentContent GetAttachmentContent(string contentHash);
        void DeleteAttachmentContent(string contentHash);
        bool HasAttachmentContent(string contentHash);
    }
}