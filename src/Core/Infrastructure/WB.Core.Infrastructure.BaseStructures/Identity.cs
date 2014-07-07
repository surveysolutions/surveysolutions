using System;

namespace WB.Core.Infrastructure.BaseStructures
{
    public class Identity
    {
        protected bool Equals(Identity other)
        {
            return this.Id.Equals(other.Id) && Equals(this.RosterVector, other.RosterVector);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (this.Id.GetHashCode()*397) ^ (this.RosterVector != null ? this.RosterVector.GetHashCode() : 0);
            }
        }

        // should be shared
        public Guid Id { get; private set; }
        public decimal[] RosterVector { get; private set; }
        public Identity(Guid id, decimal[] rosterVector)
        {
            this.Id = id;
            this.RosterVector = rosterVector;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((Identity) obj);
        }

        public static bool operator ==(Identity x, Identity y)
        {
            return x.Equals(y);
        }

        public static bool operator !=(Identity x, Identity y)
        {
            return !(x == y);
        }
    }
}