using System;
using WB.Core.SharedKernels.DataCollection.Events.Interview.Base;

namespace WB.Core.SharedKernels.DataCollection.Events.Interview
{
    public class InterviewPaused : InterviewActiveEvent
    {
        public InterviewPaused(Guid userId, DateTimeOffset originDate) 
            : base(userId, originDate)
        {
        }

        [Obsolete("Please use OriginDate property")]
        public DateTime? LocalTime { get; set; }
        [Obsolete("Please use OriginDate property")]
        public DateTime? UtcTime { get; set; }
    }
}
