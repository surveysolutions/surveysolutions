using System;
using WB.Services.Export.Events.Interview.Base;

namespace WB.Services.Export.Events.Interview
{
    [Obsolete("Since v6.0")]
    public class GroupPropagated : InterviewPassiveEvent
    {
        public Guid GroupId { get; set; }
        public decimal[] OuterScopeRosterVector { get; set; } = new decimal[0];
        public int Count { get; set; }
    }
}
