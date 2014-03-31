using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WB.Core.SharedKernels.DataCollection.Events.Interview.Base;

namespace WB.Core.SharedKernels.DataCollection.Events.Interview
{
    [Obsolete]
    public class RosterRowAdded : RosterRowEvent
    {
        public int? SortIndex { get; private set; }

        public RosterRowAdded(Guid groupId, decimal[] outerRosterVector, decimal rosterInstanceId, int? sortIndex)
            : base(groupId, outerRosterVector, rosterInstanceId)
        {
            this.SortIndex = sortIndex;
        }
    }
}
