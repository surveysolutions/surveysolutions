using System;
using WB.Core.SharedKernels.DataCollection.Commands.Interview.Base;

namespace WB.Core.SharedKernels.DataCollection.Commands.Interview
{
    public class MarkInterviewAsReceivedByInterviewer : InterviewCommand
    {
        public string DeviceId { get; set; }
        
        public MarkInterviewAsReceivedByInterviewer(Guid interviewId, string deviceId, Guid userId)
            : base(interviewId, userId)
        {
            this.DeviceId = deviceId;
        }
    }
}
