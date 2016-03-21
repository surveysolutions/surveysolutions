using System.Linq;
using System.Net.Mime;

namespace WB.Core.SharedKernels.SurveyManagement.Views.Interview
{
    public class InterviewAttachmentViewModel
    {
        private readonly string[] imageContentTypes = { "image/png", "image/jpg", "image/gif", "image/jpeg", "image/pjpeg" };

        public InterviewAttachmentViewModel(string contentId, string contentType, string contentName)
        {
            this.ContentId = contentId;
            this.ContentType = contentType;
            this.ContentName = contentName;
        }

        public string ContentId { get; set; }
        public string ContentType { get; set; }

        public string ContentName { get; set; }

        public bool IsImage => this.imageContentTypes.Contains(this.ContentType);
    }
    
}
