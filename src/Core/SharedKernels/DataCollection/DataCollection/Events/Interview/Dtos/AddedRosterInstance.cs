using System;
using System.Linq;
using WB.Core.GenericSubdomains.Portable;

namespace WB.Core.SharedKernels.DataCollection.Events.Interview.Dtos
{
    /// <remarks>Make sure not to reuse this class on read or write side. Use your own copies.</remarks>
    public class AddedRosterInstance : RosterInstance
    {
        public int? SortIndex { get; private set; }

        public AddedRosterInstance(Guid groupId, decimal[] outerRosterVector, decimal rosterInstanceId, int? sortIndex)
            : base(groupId, outerRosterVector, rosterInstanceId)
        {
            this.SortIndex = sortIndex;
        }

        public Identity GetIdentity()
        {
            var rosterVector = new RosterVector(this.OuterRosterVector.Concat(RosterInstanceId.ToEnumerable()));
            return new Identity(this.GroupId, rosterVector);
        }
    }
}