namespace WB.Core.SharedKernels.SurveyManagement.Views.QuestionnaireAttachments
{
    public class QuestionnaireAttachmentContent
    {
        public virtual string AttachmentId { get; set; }
        public virtual byte[] Content { get; set; }
    }
}