using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;

namespace WB.Core.SharedKernels.DataCollection.DataTransferObjects.Synchronization
{
    public class RosterSynchronizationDto
    {
        public RosterSynchronizationDto(Guid rosterId, decimal[] outerScopePropagationVector, decimal rosterInstanceId, int? sortIndex, string rosterTitle)
        {
            this.RosterId = rosterId;
            this.OuterScopePropagationVector = outerScopePropagationVector;
            this.RosterInstanceId = rosterInstanceId;
            this.SortIndex = sortIndex;
            this.RosterTitle = rosterTitle;
        }

        public Guid RosterId { get; private set; }
        public decimal[] OuterScopePropagationVector { get; private set; }
        public decimal RosterInstanceId { get; private set; }
        public int? SortIndex { get; private set; }
        public string RosterTitle { get; private set; }
    }
}
