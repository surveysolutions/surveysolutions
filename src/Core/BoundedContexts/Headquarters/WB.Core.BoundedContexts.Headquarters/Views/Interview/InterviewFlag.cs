using System;

namespace WB.Core.BoundedContexts.Headquarters.Views.Interview
{
    public class InterviewFlag
    {
        public virtual Guid InterviewId { get; set; }
        public virtual Guid EntityId { get; set; }
        public virtual string RosterVector { get; set; }

        public override int GetHashCode() 
            => this.InterviewId.GetHashCode() ^ this.EntityId.GetHashCode() ^ this.RosterVector.GetHashCode();
        
        public override bool Equals(object other)
        {
            if (other == null) return false;
            if (object.ReferenceEquals(this, other)) return true;
            
            return this.Equals(other as InterviewFlag);
        }
        public virtual bool Equals(InterviewFlag other)
        {
            if (other == null) return false;
            if (object.ReferenceEquals(this, other)) return true;

            return this.InterviewId == other.InterviewId && this.EntityId == other.EntityId &&
                   this.RosterVector == other.RosterVector;
        }
    }
}
