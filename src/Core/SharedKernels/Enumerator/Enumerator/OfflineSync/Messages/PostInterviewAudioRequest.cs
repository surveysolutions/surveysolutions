using System;

namespace WB.Core.SharedKernels.Enumerator.OfflineSync.Messages
{
    public class PostInterviewAudioRequest : ICommunicationMessage
    {
        public Guid InterviewId { get; set; }
        public string FileName { get; set; }
        public string ContentType { get; set; }
        public byte[] Content { get; set; }

        public PostInterviewAudioRequest(Guid interviewId, string fileName, string contentType, byte[] content)
        {
            InterviewId = interviewId;
            FileName = fileName;
            ContentType = contentType;
            Content = content;
        }
    }
}
