namespace WB.Core.SharedKernels.SurveyManagement.Views.Questionnaire
{
    public class AttachmentInfoView
    {
        public AttachmentInfoView(string contentHash, string contentType)
        {
            this.ContentHash = contentHash;
            this.ContentType = contentType;
        }

        public string ContentHash { get; private set; }
        public string ContentType { get; private set; }
    }
}