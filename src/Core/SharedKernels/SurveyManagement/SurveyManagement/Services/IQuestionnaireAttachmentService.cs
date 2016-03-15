namespace WB.Core.SharedKernels.SurveyManagement.Services
{
    public interface IQuestionnaireAttachmentService
    {
        void SaveAttachment(string attachmentHash, string contentType, byte[] content);
        byte[] GetAttachment(string attachmentHash);
        void DeleteAttachment(string attachmentHash);
        bool HasAttachment(string attachmentHash);
    }
}