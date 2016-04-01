namespace WB.Core.SharedKernels.SurveyManagement.Views.Questionnaire
{
    public class AttachmentContent
    {
        public virtual string ContentHash { get; set; }
        public virtual string ContentType { get; set; }
        public virtual byte[] Content { get; set; }
    }
}