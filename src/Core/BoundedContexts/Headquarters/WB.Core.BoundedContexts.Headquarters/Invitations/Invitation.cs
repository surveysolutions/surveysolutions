using System;
using System.Collections.Generic;
using WB.Core.BoundedContexts.Headquarters.Assignments;
using WB.Core.BoundedContexts.Headquarters.Views;
using WB.Core.BoundedContexts.Headquarters.Views.Interview;
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

        public virtual string InterviewId { get; protected set; }

        public virtual string Token { get; protected set; }

        public virtual string InvitationEmailId { get; protected set; }

        public virtual DateTime? SentOnUtc { get; protected set; }

        public virtual string ResumePassword { get; protected set; }

        public virtual Assignment Assignment { get; protected set; }

        public virtual DateTime? LastReminderSentOnUtc { get; protected set; }

        public virtual string LastReminderEmailId { get; protected set; }

        public virtual int NumberOfRemindersSent  { get; protected set; }

        public virtual InterviewSummary Interview { get; protected set; }
        
        public virtual string LastRejectedInterviewEmailId { get; protected set; }
        
        public virtual bool IsWithAssignmentResolvedByPassword() => Token.Length > 0 && Token[0] == 'I';

        public virtual void SetToken(string token, TokenKind? tokenKind = TokenKind.AssignmentResolvedByToken)
        {
            if (tokenKind == TokenKind.AssignmentResolvedByPassword)
            {
                this.Token = "I" + token;
            }
            else
            {
                this.Token = token;
            }
        }

        public virtual void InvitationWasSent(string emailId)
        {
            this.InvitationEmailId = emailId;
            this.SentOnUtc = DateTime.UtcNow;
        }

        public virtual void ReminderWasSent(string emailId)
        {
            this.LastReminderEmailId = emailId;
            this.LastReminderSentOnUtc = DateTime.UtcNow;
            this.NumberOfRemindersSent++;
        }

        public virtual void InterviewWasCreated(string interviewId)
        {
            this.InterviewId = interviewId;
        }

        public virtual void UpdateAssignmentId(int assignmentId)
        {
            this.AssignmentId = assignmentId;
        }

        public virtual void RejectedReminderSent(string emailId, int lastInterviewCommentedStatusId)
        {
            this.LastRejectedInterviewEmailId = emailId;
            this.LastRejectedStatusOrder = lastInterviewCommentedStatusId;
        }

        public virtual int? LastRejectedStatusOrder { get; protected set; }

        public virtual void RejectedReminderWasNotSent()
        {
            this.LastRejectedStatusOrder = null;
        }
    }

    public enum TokenKind
    {
        AssignmentResolvedByPassword,
        AssignmentResolvedByToken
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
