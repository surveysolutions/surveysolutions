using System;
using System.Diagnostics;
using WB.Core.GenericSubdomains.Portable;

namespace WB.Core.SharedKernels.DataCollection
{
    /// <summary>
    /// Full identity of group or question: id and roster vector.
    /// </summary>
    [DebuggerDisplay("{ToString()}")]
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

        public Guid Id { get; }

        public RosterVector RosterVector { get; }

        public Identity(Guid id, RosterVector rosterVector)
        {
            this.Id = id;
            this.RosterVector = rosterVector ?? RosterVector.Empty;
        }

        public static Identity Create(Guid id, RosterVector rosterVector) => new Identity(id, rosterVector);

        public static Identity Parse(string value)
        {
            var id = Guid.Parse(value.Substring(0, 32));
            var rosterVector = RosterVector.Parse(value.Substring(32));

            return Create(id, rosterVector);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return this.Equals((Identity) obj);
        }

        public bool Equals(Guid id, RosterVector rosterVector) => this.Equals(new Identity(id, rosterVector));

        public static bool operator ==(Identity a, Identity b)
        {
            if (ReferenceEquals(a, b))
                return true;

            if (((object)a == null) || ((object)b == null))
                return false;

            return a.Equals(b);
        }

        public static bool operator !=(Identity a, Identity b) => !(a == b);

        public override string ToString() => $"{this.Id.FormatGuid()}{this.RosterVector}";
    }
}