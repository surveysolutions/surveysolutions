using System;

namespace WB.Core.SharedKernels.DataCollection.WebApi
{
    public class PostFileApiView
    {
        public Guid InterviewId { get; set; }
        public string FileName { get; set; }
        public string Data { get; set; }
        public string ContentType { get; set; }
    }
}
