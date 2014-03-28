using WB.Core.SharedKernels.DataCollection.Events.Interview.Base;
using WB.Core.SharedKernels.DataCollection.Events.Interview.Dtos;

namespace WB.Core.SharedKernels.DataCollection.Events.Interview
{
    public class RosterRowsRemoved : InterviewPassiveEvent
    {
        public RosterRowIdentity[] Rows { get; private set; }

        public RosterRowsRemoved(RosterRowIdentity[] rows)
        {
            this.Rows = rows;
        }
    }
}