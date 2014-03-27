using System;

namespace WB.Core.SharedKernels.DataCollection.Events.Interview.Dtos
{
    /// <remarks>Make sure not to reuse this class on read or write side. Use your own copies.</remarks>
    public struct RosterRowIdentity
    {
        public Guid GroupId { get; private set; }
        public decimal[] OuterRosterVector { get; private set; }
        public decimal RosterInstanceId { get; private set; }

        public RosterRowIdentity(Guid groupId, decimal[] outerRosterVector, decimal rosterInstanceId)
            : this()
        {
            this.GroupId = groupId;
            this.OuterRosterVector = outerRosterVector;
            this.RosterInstanceId = rosterInstanceId;
        }
    }
}