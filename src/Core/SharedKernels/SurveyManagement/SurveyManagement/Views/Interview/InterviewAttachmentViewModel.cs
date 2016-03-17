using System.Linq;

namespace WB.Core.SharedKernels.SurveyManagement.Views.Interview
{
    public class InterviewAttachmentViewModel
    {
        private readonly string[] imageContentTypes = { "image/png", "image/jpg", "image/gif", "image/jpeg", "image/pjpeg" };

        public InterviewAttachmentViewModel(string contentId, string contentType)
        {
            this.ContentId = contentId;
            this.ContentType = contentType;
        }

        public string ContentId { get; set; }
        public string ContentType { get; set; }
        
        public bool IsImage => this.imageContentTypes.Contains(this.ContentType);
    }
    
}
