using System;

namespace WB.Core.SharedKernel.Structures.Synchronization.SurveyManagement
{
    public class PostFileRequest
    {
        public Guid InterviewId { get; set; }
        public string FileName { get; set; }
        public string Data { get; set; }
        public string ContentType { get; set; }
    }
}