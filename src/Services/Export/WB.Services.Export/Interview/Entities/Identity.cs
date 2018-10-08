using System;
using System.Diagnostics;

namespace WB.Services.Export.Interview.Entities
{
    /// <summary>
    /// Full identity of group or question: id and roster vector.
    /// </summary>
    [DebuggerDisplay("{" + nameof(ToString) + "()}")]
    public sealed class Identity
    {
        private int? hashCode;

        private bool Equals(Identity other)
        {
            return this.Id == other.Id && this.RosterVector.Identical(other.RosterVector);
        }

        public override int GetHashCode()
        {
            if (!this.hashCode.HasValue)
            {
                this.hashCode = this.Id.GetHashCode() ^ this.RosterVector.GetHashCode();
            }

            return this.hashCode.Value;
        }

        public Guid Id { get; private set; }

        public RosterVector RosterVector { get; private set; }

        // ReSharper disable once UnusedMember.Local 
        private Identity() { }

        public Identity(Guid id, RosterVector rosterVector)
        {
            this.Id = id;
            this.RosterVector = rosterVector ?? RosterVector.Empty;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return this.Equals((Identity)obj);
        }

        public bool Equals(Guid id, RosterVector rosterVector) => id == this.Id && this.RosterVector.Identical(rosterVector);

        public bool Equals(Guid id, RosterVector rosterVector, int length) => id == this.Id && this.RosterVector.Identical(rosterVector, length);

        public static bool operator ==(Identity a, Identity b)
        {
            if (ReferenceEquals(a, b))
                return true;

            if (((object)a == null) || ((object)b == null))
                return false;

            return a.Equals(b);
        }

        public static bool operator !=(Identity a, Identity b) => !(a == b);

        public override string ToString() => $"{this.Id:N}{this.RosterVector}";
    }
}
