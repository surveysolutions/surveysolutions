using System;
using System.Diagnostics;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.SharedKernels.DataCollection;

namespace WB.Core.BoundedContexts.Headquarters.Views.Interview
{
    [DebuggerDisplay("{ToString()}")]
    public class InterviewStateIdentity
    {
        public Guid Id { get; set; }
        public int[] RosterVector { get; set; }

        private int? hashCode;

        private bool Equals(InterviewStateIdentity other) => this.Id == other.Id && this.RosterVector.SequenceEqual(other.RosterVector);

        public override int GetHashCode()
        {
            if (!this.hashCode.HasValue)
            {
                this.hashCode = this.Id.GetHashCode() ^ this.RosterVector.GetHashCode();
            }

            return this.hashCode.Value;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return this.Equals((InterviewStateIdentity)obj);
        }

        public static bool operator ==(InterviewStateIdentity a, InterviewStateIdentity b)
        {
            if (ReferenceEquals(a, b))
                return true;

            if (((object)a == null) || ((object)b == null))
                return false;

            return a.Equals(b);
        }

        public static bool operator !=(InterviewStateIdentity a, InterviewStateIdentity b) => !(a == b);

        public override string ToString() => $"{this.Id}{(this.RosterVector.Length > 0 ? "_" + string.Join("-", this.RosterVector) : string.Empty)}";

        public static InterviewStateIdentity Create(Identity identity) => Create(identity.Id, identity.RosterVector);
        public static InterviewStateIdentity Create(Guid id, RosterVector rosterVector) => new InterviewStateIdentity
        {
            Id = id,
            RosterVector = rosterVector
        };
    }
}