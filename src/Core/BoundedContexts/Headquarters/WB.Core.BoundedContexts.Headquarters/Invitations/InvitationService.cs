using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using WB.Core.BoundedContexts.Headquarters.Assignments;
using WB.Core.BoundedContexts.Headquarters.Views;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;

namespace WB.Core.BoundedContexts.Headquarters.Invitations
{
    public class InvitationService : IInvitationService
    {
        private readonly IPlainStorageAccessor<Invitation> invitationStorage;
        private readonly IPlainKeyValueStorage<InvitationDistributionStatus> invitationsDistributionStatusStorage;
        private static CancellationTokenSource cancellationTokenSource;

        public InvitationService(
            IPlainStorageAccessor<Invitation> invitationStorage,
            IPlainKeyValueStorage<InvitationDistributionStatus> invitationsDistributionStatusStorage)
        {
            this.invitationStorage = invitationStorage;
            this.invitationsDistributionStatusStorage = invitationsDistributionStatusStorage;
        }

        public void CreateInvitationForWebInterview(Assignment assignment)
        {
            var hasEmail = !string.IsNullOrWhiteSpace(assignment.Email);
            var hasPassword = !string.IsNullOrWhiteSpace(assignment.Password);
            var isPrivateAssignment = (assignment.Quantity ?? 1) == 1;

            /*
            Quantity  Password 	    Email 	    
            1         empty 	    empty      -
           -1 	      empty 	    not empty  -
           -1 	      not empty 	not empty  -
           */
            if (isPrivateAssignment && !hasEmail && !hasPassword)
                return;

            if (!isPrivateAssignment && hasEmail)
                return;

            var assignmentId = assignment.Id;
            var invitation = new Invitation(assignmentId);
            var questionnaireHash = assignment.QuestionnaireId.GetHashCode();

            if (!isPrivateAssignment)
            {
                /*
                Quantity  Password 	    Email 	    
                -1 	      empty 	    empty      Public link, no password
                -1 	      not empty 	empty      Public link, with password
                */
                var token = TokenGenerator.Instance.Generate(questionnaireHash + 293 * assignmentId);
                invitation.SetToken(token);
            }
            else
            {
                /*
                Quantity  Password 	    Email 	    
                1 	      not empty 	empty      Public link, unique passwords. Token should be unique for all assignments
                1          empty 	    not empty  Private link, no password
                1 	      not empty 	not empty  Private link, with password
                */
                if (!hasEmail)
                {
                    var token = TokenGenerator.Instance.Generate(questionnaireHash);
                    invitation.SetToken(token);
                }
                else
                {
                    var hash = questionnaireHash + assignment.Email.GetHashCode() + assignmentId;
                    var token = TokenGenerator.Instance.Generate(hash);
                    invitation.SetToken(token);
                }
            }

            invitationStorage.Store(invitation, null);
        }

        public int GetCountOfInvitations(QuestionnaireIdentity questionnaireIdentity)
        {
            return invitationStorage.Query(_ => _.Count(x => x.Assignment.Archived == false));
        }

        public int GetCountOfNotSentInvitations(QuestionnaireIdentity questionnaireIdentity)
        {
            return invitationStorage.Query(_ => _.Count(x =>
                x.Assignment.QuestionnaireId.QuestionnaireId == questionnaireIdentity.QuestionnaireId &&
                x.Assignment.QuestionnaireId.Version == questionnaireIdentity.Version &&
                x.Assignment.Email != null && x.Assignment.Email != string.Empty && 
                x.Assignment.Archived == false &&
                x.SentOnUtc == null));
        }

        public int GetCountOfSentInvitations(QuestionnaireIdentity questionnaireIdentity)
        {
            return invitationStorage.Query(_ => _.Count(x =>
                x.Assignment.QuestionnaireId.QuestionnaireId == questionnaireIdentity.QuestionnaireId &&
                x.Assignment.QuestionnaireId.Version == questionnaireIdentity.Version &&
                x.Assignment.Archived == false &&
                x.SentOnUtc != null));
        }

        public List<int> GetInvitationIdsToSend(QuestionnaireIdentity questionnaireIdentity)
        {
            return invitationStorage.Query(_ => _.Where(x =>
                x.Assignment.QuestionnaireId.QuestionnaireId == questionnaireIdentity.QuestionnaireId &&
                x.Assignment.QuestionnaireId.Version == questionnaireIdentity.Version &&
                x.Assignment.Email != null && x.Assignment.Email != string.Empty &&
                x.Assignment.Archived == false &&
                x.SentOnUtc == null).Select(x => x.Id).ToList());
        }

        public InvitationDistributionStatus GetEmailDistributionStatus()
        {
            return this.invitationsDistributionStatusStorage.GetById(AppSetting.InvitationsDistributionStatus);
        }

        public void RequestEmailDistributionProcess(QuestionnaireIdentity questionnaireIdentity, string identityName,
            string baseUrl, string questionnaireTitle)
        {
            var status = new InvitationDistributionStatus
            {
                QuestionnaireIdentity = questionnaireIdentity,
                ResponsibleName = identityName,
                Status = InvitationProcessStatus.Queued,
                BaseUrl = baseUrl,
                QuestionnaireTitle = questionnaireTitle
            };
            this.invitationsDistributionStatusStorage.Store(status, AppSetting.InvitationsDistributionStatus);
        }

        public void StartEmailDistribution()
        {
            cancellationTokenSource = new CancellationTokenSource();
            var status = this.GetEmailDistributionStatus();
            status.Status = InvitationProcessStatus.InProgress;
            status.TotalCount = this.GetCountOfNotSentInvitations(status.QuestionnaireIdentity);
            status.StartedDate = DateTime.UtcNow;
            this.invitationsDistributionStatusStorage.Store(status, AppSetting.InvitationsDistributionStatus);
        }

        public void CompleteEmailDistribution()
        {
            var status = this.GetEmailDistributionStatus();
            status.Status = InvitationProcessStatus.Done;
            this.invitationsDistributionStatusStorage.Store(status, AppSetting.InvitationsDistributionStatus);
            cancellationTokenSource = null;
        }

        public void EmailDistributionFailed()
        {
            var status = this.GetEmailDistributionStatus();
            status.Status = InvitationProcessStatus.Failed;
            this.invitationsDistributionStatusStorage.Store(status, AppSetting.InvitationsDistributionStatus);
            cancellationTokenSource = null;
        }

        public void EmailDistributionCanceled()
        {
            var status = this.GetEmailDistributionStatus();
            status.Status = InvitationProcessStatus.Canceled;
            this.invitationsDistributionStatusStorage.Store(status, AppSetting.InvitationsDistributionStatus);
            cancellationTokenSource = null;
        }

        public void CancelEmailDistribution()
        {
            cancellationTokenSource.Cancel();
        }

        public CancellationToken GetCancellationToken()
        {
            return cancellationTokenSource.Token;
        }

        public Invitation GetInvitation(int invitationId)
        {
            return invitationStorage.GetById(invitationId);
        }

        public void InvitationWasNotSent(int invitationId, int assignmentId, string email, string reason)
        {
            var status = this.GetEmailDistributionStatus();
            status.WithErrorsCount++;
            status.Errors.Add(new InvitationSendError(invitationId, assignmentId, email, reason));
            this.invitationsDistributionStatusStorage.Store(status, AppSetting.InvitationsDistributionStatus);
        }

        public void MarkInvitationAsSent(int invitationId, string emailId)
        {
            var invitation = this.GetInvitation(invitationId);
            invitation.InvitationWasSent(emailId);
            invitationStorage.Store(invitation, invitationId);
            var status = this.GetEmailDistributionStatus();
            status.ProcessedCount++;
            this.invitationsDistributionStatusStorage.Store(status, AppSetting.InvitationsDistributionStatus);
        }
    }

    public interface IInvitationService
    {
        void CreateInvitationForWebInterview(Assignment assignment);
        int GetCountOfInvitations(QuestionnaireIdentity questionnaireIdentity);
        int GetCountOfNotSentInvitations(QuestionnaireIdentity questionnaireIdentity);
        int GetCountOfSentInvitations(QuestionnaireIdentity questionnaireIdentity);
        InvitationDistributionStatus GetEmailDistributionStatus();
        List<int> GetInvitationIdsToSend(QuestionnaireIdentity questionnaireIdentity);
        Invitation GetInvitation(int invitationId);
        void InvitationWasNotSent(int invitationId, int assignmentId, string email, string reason);
        void MarkInvitationAsSent(int invitationId, string emailId);

        void RequestEmailDistributionProcess(QuestionnaireIdentity questionnaireIdentity, string identityName, string baseUrl, string questionnaireTitle);
        void StartEmailDistribution();
        void CompleteEmailDistribution();
        void EmailDistributionFailed();
        void EmailDistributionCanceled();
        void CancelEmailDistribution();
        CancellationToken GetCancellationToken();
    }
}
