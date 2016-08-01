using System;
using System.Diagnostics;
using System.Linq;

namespace WB.Core.SharedKernels.DataCollection.Events.Interview.Dtos
{
    /// <remarks>Make sure not to reuse this class on read or write side. Use your own copies.</remarks>
    [DebuggerDisplay("GroupId = {GroupId}, RosterInstanceId = {RosterInstanceId}, OuterRosterVector = {OuterRosterVector}")]
    public class RosterInstance
    {
        public Guid GroupId { get; private set; }
        public decimal[] OuterRosterVector { get; private set; }
        public decimal RosterInstanceId { get; private set; }

        public RosterInstance(Guid groupId, decimal[] outerRosterVector, decimal rosterInstanceId)
        {
            this.GroupId = groupId;
            this.OuterRosterVector = outerRosterVector;
            this.RosterInstanceId = rosterInstanceId;
        }

        public static RosterInstance CreateFromIdentity(Identity identity)
            => new RosterInstance(identity.Id, identity.RosterVector.Shrink(identity.RosterVector.Length - 1), identity.RosterVector.Coordinates.Last());
    }
}