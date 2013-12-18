using System;
using WB.Core.SharedKernels.DataCollection.Events.Interview.Base;

namespace WB.Core.SharedKernels.DataCollection.Events.Interview
{
    public class RosterTitleChanged : RosterRowEvent
    {
        public string Title { private set; get; }

        public RosterTitleChanged(Guid groupId, decimal[] outerRosterVector, decimal rosterInstanceId, string title)
            : base(groupId, outerRosterVector, rosterInstanceId)
        {
            this.Title = title;
        }
    }
}