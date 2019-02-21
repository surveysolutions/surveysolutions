using System;

namespace WB.Core.BoundedContexts.Headquarters.Invitations
{
    public class Invitation
    {
        public Invitation()
        {
        }

        public Invitation(int assignmentId)
        {
            this.AssignmentId = assignmentId;
        }

        public virtual int Id { get; protected set; }

        public virtual int AssignmentId { get; protected set; }

        public virtual Guid? InterviewId { get; protected set; }

        public virtual string Token { get; protected set; }

        public virtual string AccessToken { get; protected set; }

        public virtual DateTime? SentOnUtc { get; protected set; }

        public virtual void SetToken(string token)
        {
            this.Token = token;
        }

        public virtual void InvitationWasSent(string token)
        {
            this.SentOnUtc = DateTime.UtcNow;
        }
    }
}
