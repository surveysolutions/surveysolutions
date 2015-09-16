using System;
using System.Diagnostics;
using WB.Core.GenericSubdomains.Portable;

namespace WB.Core.SharedKernels.DataCollection
{
    /// <summary>
    /// Full identity of group or question: id and roster vector.
    /// </summary>
    /// <remarks>
    /// Is used only internally to simplify return of id and roster vector as return value
    /// and to reduce parameters count in calculation methods.
    /// Should not be made public or be used in any form in events or commands.
    /// </remarks>
    public class Identity
    {
        protected bool Equals(Identity other)
        {
            return this.Id.Equals(other.Id) && this.RosterVector.Identical(other.RosterVector);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hc = this.RosterVector.Length;
                for (int i = 0; i < this.RosterVector.Length; ++i)
                {
                    hc = unchecked(hc * 13 + this.RosterVector[i].GetHashCode());
                }

                return hc + this.Id.GetHashCode() * 29;
            }
        }

        public Guid Id { get; private set; }

        public RosterVector RosterVector { get; private set; }

        public Identity(Guid id, RosterVector rosterVector)
        {
            this.Id = id;
            this.RosterVector = rosterVector ?? new RosterVector(new decimal[]{});
        }

        public override string ToString()
        {
            return ConversionHelper.ConvertIdentityToString(this);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return this.Equals((Identity) obj);
        }

        public bool Equals(Guid id, RosterVector rosterVector)
        {
            return Equals(new Identity(id, rosterVector));
        }
    }
}