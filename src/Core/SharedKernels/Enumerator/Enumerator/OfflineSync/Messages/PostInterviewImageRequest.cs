using System;

namespace WB.Core.SharedKernels.Enumerator.OfflineSync.Messages
{
    public class PostInterviewImageRequest
    {
        public Guid InterviewId { get; set;  }
        public string FileName { get; set; }
        public byte[] Content { get; set; }

        public PostInterviewImageRequest(Guid interviewId, string fileName, byte[] content)
        {
            InterviewId = interviewId;
            FileName = fileName;
            Content = content;
        }
    }
}
