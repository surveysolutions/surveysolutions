using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WB.Core.SharedKernels.DataCollection.Events.Interview.Base;

namespace WB.Core.SharedKernels.DataCollection.Events.Interview
{
    public class RosterRowDeleted: InterviewPassiveEvent
    {
        public RosterRowDeleted(Guid groupId, decimal[] outerRosterVector, decimal rosterInstanceId)
        {
            this.GroupId = groupId;
            this.OuterRosterVector = outerRosterVector;
            this.RosterInstanceId = rosterInstanceId;
        }

        public Guid GroupId { get; private set; }
        public decimal[] OuterRosterVector { get; private set; }
        public decimal RosterInstanceId { get; private set; }
    }
}
