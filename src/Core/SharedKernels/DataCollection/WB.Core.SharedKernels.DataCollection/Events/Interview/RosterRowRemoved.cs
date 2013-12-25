using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WB.Core.SharedKernels.DataCollection.Events.Interview.Base;

namespace WB.Core.SharedKernels.DataCollection.Events.Interview
{
    public class RosterRowRemoved : RosterRowEvent
    {
        public RosterRowRemoved(Guid groupId, decimal[] outerRosterVector, decimal rosterInstanceId)
            : base(groupId, outerRosterVector, rosterInstanceId) {}
    }
}
