using System;
using System.Diagnostics;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.SharedKernels.DataCollection;

namespace WB.Core.BoundedContexts.Headquarters.Views.Interview
{
    [DebuggerDisplay("{ToString()}")]
    public class InterviewStateIdentity: IEquatable<InterviewStateIdentity>
    {
        public Guid Id { get; set; }
        public int[] RosterVector { get; }

        private readonly int hashCode;

        public InterviewStateIdentity()
        {
        }

        public InterviewStateIdentity(Guid id, int[] rosterVector)
        {
            Id = id;
            RosterVector = rosterVector;
            
            var rosterVectorHashCode = RosterVector.Length;
            unchecked
            {
                for (var index = 0; index < RosterVector.Length; index++)
                {
                    rosterVectorHashCode = rosterVectorHashCode * 13 + RosterVector[index];
                }

                hashCode = (Id.GetHashCode() * 397) ^ rosterVectorHashCode;
            }
        }

        public bool Equals(InterviewStateIdentity other)
        {
            if (other == null)
                return false;
            return this.Id == other.Id && this.RosterVector.SequenceEqual(other.RosterVector);
        }
        

        public override int GetHashCode()
        {
            return this.hashCode;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return this.Equals((InterviewStateIdentity)obj);
        }

        public static bool operator == (InterviewStateIdentity a, InterviewStateIdentity b)
        {
            if (ReferenceEquals(a, b))
                return true;

            if (((object)a == null) || ((object)b == null))
                return false;

            return a.Equals(b);
        }

        public static bool operator != (InterviewStateIdentity a, InterviewStateIdentity b) => !(a == b);

        public override string ToString() => $"{this.Id}{(this.RosterVector.Length > 0 ? "_" + string.Join("-", this.RosterVector) : string.Empty)}";

        public static InterviewStateIdentity Create(Identity identity) => 
            new InterviewStateIdentity(id: identity.Id, rosterVector: identity.RosterVector);
        public static InterviewStateIdentity Create(Guid id, RosterVector rosterVector) =>
            new InterviewStateIdentity(id: id, rosterVector: rosterVector);

        public static InterviewStateIdentity Create(Guid id, params int[] rosterVector) => 
            new InterviewStateIdentity(id: id, rosterVector: rosterVector);
    }
}
