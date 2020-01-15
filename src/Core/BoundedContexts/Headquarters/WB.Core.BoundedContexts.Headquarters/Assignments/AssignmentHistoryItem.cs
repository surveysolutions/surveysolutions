using System;
using System.Runtime.Serialization;

namespace WB.Core.BoundedContexts.Headquarters.Assignments
{
    public class AssignmentHistoryItem
    {
        public AssignmentHistoryItem(AssignmentHistoryAction action, string actor, DateTime utcDate)
        {
            this.Action = action;
            this.ActorName = actor;
            this.UtcDate = utcDate;
        }

        [DataMember(IsRequired = true)]
        public AssignmentHistoryAction Action { get; set; }

        [DataMember(IsRequired = true)]
        public string ActorName { get; set; }

        [DataMember(IsRequired = true)]
        public DateTime UtcDate { get; set; }

        public object AdditionalData { get; set; }
    }
}