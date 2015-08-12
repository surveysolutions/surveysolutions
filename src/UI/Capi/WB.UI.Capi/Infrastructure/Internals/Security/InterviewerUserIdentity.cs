using System;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure;

namespace WB.UI.Capi.Infrastructure.Internals.Security
{
    public class InterviewerUserIdentity : IUserIdentity
    {
        public InterviewerUserIdentity(string name, Guid userId)
        {
            this.Name = name;
            this.UserId = userId;
        }

        public string Name { get; private set; }
        public Guid UserId { get; private set; }
        public string Password { get; private set; }

        protected bool Equals(InterviewerUserIdentity other)
        {
            return this.UserId.Equals(other.UserId);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((InterviewerUserIdentity)obj);
        }

        public override int GetHashCode()
        {
            return this.UserId.GetHashCode();
        }
    }
}