using System;
using System.Collections.Generic;
using WB.Core.BoundedContexts.Headquarters.Assignments;
using WB.Core.BoundedContexts.Headquarters.Views;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;

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

        public virtual string EmailId { get; protected set; }

        public virtual string ResumePassword { get; protected set; }

        public virtual DateTime? SentOnUtc { get; protected set; }

        public virtual Assignment Assignment { get; protected set; }

        public virtual void SetToken(string token)
        {
            this.Token = token;
        }

        public virtual void InvitationWasSent(string emailId)
        {
            this.EmailId = emailId;
            this.SentOnUtc = DateTime.UtcNow;
        }
    }

    public class InvitationDistributionStatus : AppSetting
    {
        public string ResponsibleName { get; set; }
        public QuestionnaireIdentity QuestionnaireIdentity { get; set; }
        public DateTime StartedDate { get; set; }
        public long ProcessedCount { get; set; }
        public long WithErrorsCount { get; set; }
        public long TotalCount { get; set; }
        public InvitationProcessStatus Status { get; set; }
        public List<InvitationSendError> Errors { get; set; } = new List<InvitationSendError>();
        public string BaseUrl { get; set; }
        public string QuestionnaireTitle { get; set; }
    }

    public class InvitationSendError
    {
        public int InvitationId { get; }
        public int AssignmentId { get; }
        public string Email { get; }
        public string Error { get; }

        public InvitationSendError(int invitationId, int assignmentId, string email, string error)
        {
            InvitationId = invitationId;
            AssignmentId = assignmentId;
            Email = email;
            Error = error;
        }
    }

    public enum InvitationProcessStatus
    {
        Queued = 0,
        InProgress = 1,
        Done = 2,
        Failed = 3,
        Canceled = 4
    }
}
