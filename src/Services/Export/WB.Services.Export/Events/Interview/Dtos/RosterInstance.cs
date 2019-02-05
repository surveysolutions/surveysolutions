using System;
using System.Diagnostics;
using System.Linq;

namespace WB.Services.Export.Events.Interview.Dtos
{
    /// <remarks>Make sure not to reuse this class on read or write side. Use your own copies.</remarks>
    [DebuggerDisplay("GroupId = {GroupId}, RosterInstanceId = {RosterInstanceId}, OuterRosterVector = {OuterRosterVector}")]
    public class RosterInstance
    {
        public Guid GroupId { get; private set; }
        public decimal[] OuterRosterVector { get; private set; }
        public decimal RosterInstanceId { get; private set; }

        private int? hashCode;

        private bool Equals(RosterInstance other)
        {
            return this.GroupId == other.GroupId &&
                   this.RosterInstanceId == other.RosterInstanceId &&
                   this.OuterRosterVector.Length == other.OuterRosterVector.Length &&
                   Enumerable.SequenceEqual(this.OuterRosterVector, other.OuterRosterVector);
        }

        public override int GetHashCode()
        {
            if (!this.hashCode.HasValue)
            {
                int hashOfOuterRosterVector = this.OuterRosterVector.Aggregate(0, (current, el) => current ^ el.GetHashCode());
                this.hashCode = this.GroupId.GetHashCode() ^ this.RosterInstanceId.GetHashCode() ^ hashOfOuterRosterVector;
            }

            return this.hashCode.Value;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return this.Equals((RosterInstance)obj);
        }
        
        public static bool operator ==(RosterInstance a, RosterInstance b)
        {
            if (ReferenceEquals(a, b))
                return true;

            if (((object)a == null) || ((object)b == null))
                return false;

            return a.Equals(b);
        }

        public static bool operator !=(RosterInstance a, RosterInstance b) => !(a == b);
    }
}
