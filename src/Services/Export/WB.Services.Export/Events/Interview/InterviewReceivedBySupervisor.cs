using System;
using WB.Services.Export.Events.Interview.Base;

namespace WB.Services.Export.Events.Interview
{
    public class InterviewReceivedBySupervisor : InterviewPassiveEvent
    {
        public InterviewReceivedBySupervisor(DateTimeOffset? originDate)
        {
            if (originDate != default(DateTimeOffset))
            {
                this.OriginDate = originDate;
            }
        }
    }
}
