namespace WB.Core.SharedKernels.SurveyManagement.Views.Questionnaire
{
    public class QuestionnaireAttachment
    {
        public virtual string AttachmentHash { get; set; }
        public virtual string ContentType { get; set; }
        public virtual byte[] Content { get; set; }
    }
}