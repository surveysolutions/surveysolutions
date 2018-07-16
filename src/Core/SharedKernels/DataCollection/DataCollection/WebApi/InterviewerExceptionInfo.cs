using System;

namespace WB.Core.SharedKernels.DataCollection.WebApi
{
    public class InterviewerExceptionInfo
    {
        public Guid InterviewerId { get; set; }
        public string Message { get; set; }
        public string StackTrace { get; set; }
    }
}
