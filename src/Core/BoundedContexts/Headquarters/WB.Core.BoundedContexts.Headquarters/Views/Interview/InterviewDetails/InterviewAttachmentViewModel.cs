namespace WB.Core.BoundedContexts.Headquarters.Views.Interview
{
    public class InterviewAttachmentViewModel
    {
        public string ContentId { get; set; }

        public string ContentType { get; set; }

        public string ContentName { get; set; }

        public bool IsImage => this.ContentType.StartsWith("image");
    }
    
}
