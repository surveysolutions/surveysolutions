using System;
using System.Collections.Generic;
using System.Linq;

namespace WB.Core.SharedKernels.DataCollection.Implementation.Aggregates
{
    public class RosterIdentity
    {
        public Guid GroupId { get; private set; }
        public decimal[] OuterRosterVector { get; private set; }
        public decimal RosterInstanceId { get; private set; }
        public int? SortIndex { get; private set; }

        public RosterIdentity(Guid groupId, decimal[] outerRosterVector, decimal rosterInstanceId, int? sortIndex = null)
        {
            this.GroupId = groupId;
            this.OuterRosterVector = outerRosterVector;
            this.RosterInstanceId = rosterInstanceId;
            this.SortIndex = sortIndex;
        }
    }

    internal class RosterIdentityComparer : IEqualityComparer<RosterIdentity>
    {
        public bool Equals(RosterIdentity x, RosterIdentity y)
        {
            return x.GroupId == y.GroupId && x.OuterRosterVector.SequenceEqual(y.OuterRosterVector) &&
                x.RosterInstanceId == y.RosterInstanceId && ((!x.SortIndex.HasValue && !y.SortIndex.HasValue) || x.SortIndex == y.SortIndex);
        }

        public int GetHashCode(RosterIdentity obj)
        {
            int hashOfOuterRosterVector = obj.OuterRosterVector.Aggregate(0, (current, el) => current ^ el.GetHashCode());

            return obj.GroupId.GetHashCode() ^ obj.RosterInstanceId.GetHashCode() ^ (obj.SortIndex ?? 0) ^ hashOfOuterRosterVector;
        }
    }
}
