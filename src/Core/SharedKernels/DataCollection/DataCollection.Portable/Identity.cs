using System;
using System.Diagnostics;
using System.Linq;

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
    [DebuggerDisplay("Id = {Id}, RosterVector = [{string.Join(\",\", RosterVector)}]")]
    public class Identity
    {
        protected bool Equals(Identity other)
        {
            return this.Id.Equals(other.Id) && this.RosterVector.SequenceEqual(other.RosterVector);
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
            return this.Equals((Identity) obj);
        }
    }
}