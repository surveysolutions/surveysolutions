using System;
using WB.Services.Export.Events.Interview.Base;

namespace WB.Services.Export.Events.Interview
{
    public class InterviewOpenedBySupervisor : InterviewActiveEvent
    {
        public DateTime? LocalTime { get; set; }
    }
}
