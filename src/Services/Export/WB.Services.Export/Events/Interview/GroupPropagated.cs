using System;
using WB.Services.Export.Events.Interview.Base;

namespace WB.Services.Export.Events.Interview
{
    [Obsolete("Since v6.0")]
    public class GroupPropagated : InterviewPassiveEvent
    {
        public Guid GroupId { get; private set; }
        public decimal[] OuterScopeRosterVector { get; private set; }
        public int Count { get; private set; }
    }
}
