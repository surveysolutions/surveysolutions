using System;
using WB.Core.SharedKernels.DataCollection.Events.Interview.Base;

namespace WB.Core.SharedKernels.DataCollection.Events.Interview
{
    [Obsolete]
    public class RosterRowTitleChanged : RosterRowEvent
    {
        public string Title { private set; get; }

        public RosterRowTitleChanged(Guid groupId, decimal[] outerRosterVector, decimal rosterInstanceId, string title)
            : base(groupId, outerRosterVector, rosterInstanceId)
        {
            this.Title = title;
        }
    }
}