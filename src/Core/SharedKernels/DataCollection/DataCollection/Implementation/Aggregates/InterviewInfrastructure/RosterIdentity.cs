using System;
using System.Collections.Generic;
using System.Linq;
using WB.Core.GenericSubdomains.Portable;

namespace WB.Core.SharedKernels.DataCollection.Implementation.Aggregates
{
    public class RosterIdentity
    {
        public Guid GroupId { get; }
        public decimal[] OuterRosterVector { get; }
        public decimal RosterInstanceId { get;  }
        public int? SortIndex { get; }

        public RosterIdentity(Guid groupId, decimal[] outerRosterVector, decimal rosterInstanceId, int? sortIndex = null)
        {
            this.GroupId = groupId;
            this.OuterRosterVector = outerRosterVector;
            this.RosterInstanceId = rosterInstanceId;
            this.SortIndex = sortIndex;
        }

        public Identity ToIdentity()
        {
            return new Identity(GroupId, new RosterVector(OuterRosterVector).ExtendWithOneCoordinate((int)RosterInstanceId));
        }
    }

    internal class RosterIdentityComparer : IEqualityComparer<RosterIdentity>
    {
        public bool Equals(RosterIdentity x, RosterIdentity y)
        {
            if (object.ReferenceEquals(x, y))
            {
                return true;
            }

            return x.GroupId == y.GroupId &&
                   x.RosterInstanceId == y.RosterInstanceId && 
                   ((!x.SortIndex.HasValue && !y.SortIndex.HasValue) || x.SortIndex == y.SortIndex) &&
                   x.OuterRosterVector.Length == y.OuterRosterVector.Length &&
                   x.OuterRosterVector.SequenceEqual(y.OuterRosterVector);
        }

        public int GetHashCode(RosterIdentity obj)
        {
            int hashOfOuterRosterVector = obj.OuterRosterVector.Aggregate(0, (current, el) => current ^ el.GetHashCode());
            var hashcode =  obj.GroupId.GetHashCode() ^ obj.RosterInstanceId.GetHashCode() ^ (obj.SortIndex ?? 0) ^ hashOfOuterRosterVector;
            return hashcode;
        }
    }
}
