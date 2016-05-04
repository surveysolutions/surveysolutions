using System;

namespace WB.Core.SharedKernels.DataCollection.DataTransferObjects.Synchronization
{
    public class RosterSynchronizationDto
    {
        public RosterSynchronizationDto(Guid rosterId, decimal[] outerScopeRosterVector, decimal rosterInstanceId, int? sortIndex, string rosterTitle)
        {
            this.RosterId = rosterId;
            this.OuterScopeRosterVector = outerScopeRosterVector;
            this.RosterInstanceId = rosterInstanceId;
            this.SortIndex = sortIndex;
            this.RosterTitle = rosterTitle;
        }

        public Guid RosterId { get; private set; }
        public decimal[] OuterScopeRosterVector { get; private set; }
        public decimal RosterInstanceId { get; private set; }
        public int? SortIndex { get; private set; }
        public string RosterTitle { get; private set; }
    }
}
