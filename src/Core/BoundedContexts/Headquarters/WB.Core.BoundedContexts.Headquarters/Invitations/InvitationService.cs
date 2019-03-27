using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using WB.Core.BoundedContexts.Headquarters.Assignments;
using WB.Core.BoundedContexts.Headquarters.Views;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;

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
        
        public int CreateInvitationForPublicLink(Assignment assignment, string interviewId)
        {
            var invitation = new Invitation(assignment.Id);
            var hash =  assignment.QuestionnaireId.GetHashCode() * (1 + interviewId.GetHashCode()) + assignment.Id;
            var token = TokenGenerator.Instance.Generate(hash);
            invitation.SetToken(token);
            invitation.InterviewWasCreated(interviewId);
            invitationStorage.Store(invitation, null);
            return invitation.Id;
        }

        public void CreateInvitationForWebInterview(Assignment assignment)
        {
            if (assignment.WebMode == false)
            {
                return;
            }

            var hasEmail = !string.IsNullOrWhiteSpace(assignment.Email);
            var isPrivateAssignment = assignment.Quantity == 1;

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
                var token = TokenGenerator.Instance.Generate(questionnaireHash * 293 * assignmentId);
                invitation.SetToken(token);
            }
            else
            {
                /*
                Quantity  Password 	    Email 	    
                1 	      not empty 	empty      Public link, unique passwords. Token should be unique for all assignments
                1         empty 	    not empty  Private link, no password
                1 	      not empty 	not empty  Private link, with password
                */
                if (!hasEmail)
                {
                    var token = "I" + TokenGenerator.Instance.Generate(questionnaireHash);
                    invitation.SetToken(token);
                }
                else
                {
                    var hash = questionnaireHash * (1 + assignment.Email.GetHashCode()) + assignmentId;
                    var token = TokenGenerator.Instance.Generate(hash);
                    invitation.SetToken(token);
                }
            }

            invitationStorage.Store(invitation, null);
        }

        public int GetCountOfInvitations(QuestionnaireIdentity questionnaireIdentity)
        {
            return invitationStorage.Query(_ => _
                .Where(FilteredByQuestionnaire(questionnaireIdentity))
                .Where(HasEmail())
                .Where(NotArchived())
                .Count());
        }


        public int GetCountOfNotSentInvitations(QuestionnaireIdentity questionnaireIdentity)
        {
            return invitationStorage.Query(_ => _
                .Where(FilteredByQuestionnaire(questionnaireIdentity))
                .Where(HasEmail())
                .Where(NotArchived())
                .Count(x => x.SentOnUtc == null));
        }

        public int GetCountOfSentInvitations(QuestionnaireIdentity questionnaireIdentity)
        {
            return invitationStorage.Query(_ => _
                .Where(FilteredByQuestionnaire(questionnaireIdentity))
                .Where(HasEmail())
                .Where(NotArchived())
                .Count(x => x.SentOnUtc != null));
        }

        public List<int> GetInvitationIdsToSend(QuestionnaireIdentity questionnaireIdentity)
        {
            return invitationStorage.Query(_ => _
                .Where(FilteredByQuestionnaire(questionnaireIdentity))
                .Where(HasEmail())
                .Where(NotArchived())
                .Where(x => x.SentOnUtc == null)
                .Select(x => x.Id)
                .ToList());
        }

        
        public IEnumerable<int> GetPartialResponseInvitations(QuestionnaireIdentity questionnaireIdentity,
            int thresholdDays)
        {
            var thresholdDate = DateTime.UtcNow.AddDays(-thresholdDays);
            var partialResponseInvitationsWithoutReminders = invitationStorage.Query(_ => _
                .Where(FilteredByQuestionnaire(questionnaireIdentity))
                .Where(HasInterview())
                .Where(NoReminderAndInvitationIsExpired(thresholdDate))
                .Where(x => x.Interview.Status < InterviewStatus.Completed)
                .Select(x => x.Id)
                .ToList());

            var partialResponseInvitationsWithReminders = invitationStorage.Query(_ => _
                .Where(FilteredByQuestionnaire(questionnaireIdentity))
                .Where(HasInterview())
                .Where(LastReminderIsExpired(thresholdDate))
                .Where(x => x.Interview.Status < InterviewStatus.Completed)
                .Select(x => x.Id)
                .ToList());

            return partialResponseInvitationsWithoutReminders.Union(partialResponseInvitationsWithReminders).ToList();
        }

        public IEnumerable<int> GetNoResponseInvitations(QuestionnaireIdentity questionnaireIdentity, int thresholdDays)
        {
            DateTime thresholdDate = DateTime.UtcNow.AddDays(-thresholdDays);
            var noResponseInvitationsWithoutReminders = invitationStorage.Query(_ => _
                .Where(FilteredByQuestionnaire(questionnaireIdentity))
                .Where(HasNoInterview())
                .Where(NoReminderAndInvitationIsExpired(thresholdDate))
                .Select(x => x.Id)
                .ToList());

            var noResponseInvitationsWithReminders = invitationStorage.Query(_ => _
                .Where(FilteredByQuestionnaire(questionnaireIdentity))
                .Where(HasNoInterview())
                .Where(LastReminderIsExpired(thresholdDate))
                .Select(x => x.Id)
                .ToList());

            return noResponseInvitationsWithoutReminders.Union(noResponseInvitationsWithReminders).ToList();
        }

        public List<Invitation> GetInvitationsToExport(QuestionnaireIdentity questionnaireIdentity)
        {
            return invitationStorage.Query(_ => _
                .Where(FilteredByQuestionnaire(questionnaireIdentity))
                .Where(NotArchived())
                .Where(x => (x.Assignment.Quantity ?? int.MaxValue) - x.Assignment.InterviewSummaries.Count > 0)
                .ToList());
        }

        public InvitationDistributionStatus GetEmailDistributionStatus()
        {
            return this.invitationsDistributionStatusStorage.GetById(AppSetting.InvitationsDistributionStatus);
        }

        public void MarkInvitationAsReminded(int invitationId, string emailId)
        {
            var invitation = this.GetInvitation(invitationId);
            invitation.ReminderWasSent(emailId);
            invitationStorage.Store(invitation, invitationId);
        }

        public void RequestEmailDistributionProcess(QuestionnaireIdentity questionnaireIdentity, string identityName,
            string questionnaireTitle)
        {
            var status = new InvitationDistributionStatus
            {
                QuestionnaireIdentity = questionnaireIdentity,
                ResponsibleName = identityName,
                Status = InvitationProcessStatus.Queued,
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
            if (status == null) return;
            status.Status = InvitationProcessStatus.Done;
            this.invitationsDistributionStatusStorage.Store(status, AppSetting.InvitationsDistributionStatus);
            cancellationTokenSource = null;
        }

        public void EmailDistributionFailed()
        {
            var status = this.GetEmailDistributionStatus();
            if (status == null) return;
            status.Status = InvitationProcessStatus.Failed;
            this.invitationsDistributionStatusStorage.Store(status, AppSetting.InvitationsDistributionStatus);
            cancellationTokenSource = null;
        }

        public void EmailDistributionCanceled()
        {
            var status = this.GetEmailDistributionStatus();
            if (status == null) return;
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

        public void ReminderWasNotSent(int invitationId, int assignmentId, string address, string message)
        {
        }

        public IEnumerable<QuestionnaireLiteViewItem> GetQuestionnairesWithInvitations()
        {
            return invitationStorage.Query(_ => _.Select(x => x.Assignment.Questionnaire).Distinct().ToList());
        }

        public Invitation GetInvitationByToken(string token)
        {
            var uppercaseToken = token.ToUpper();
            return invitationStorage.Query(_ => _.FirstOrDefault(x => x.Token == uppercaseToken));
        }

        public Invitation GetInvitationByTokenAndPassword(string token, string password)
        {
            var uppercaseToken = token.ToUpper();
            var uppercasePassword = password.ToUpper();
            return invitationStorage.Query(_ => _.FirstOrDefault(x => x.Token == uppercaseToken && x.Assignment.Password == uppercasePassword));
        }

        public Invitation GetInvitationByAssignmentId(int assignmentId)
        {
            return invitationStorage.Query(_ => _.SingleOrDefault(x => x.AssignmentId == assignmentId));
        }

        public void InterviewWasCreated(int invitationId, string interviewId)
        {
            var invitation = this.GetInvitation(invitationId);
            invitation.InterviewWasCreated(interviewId);
            invitationStorage.Store(invitation, invitationId);
        }

        public bool IsValidTokenAndPassword(string token, string password)
        {
            var uppercaseToken = token.ToUpper();
            var uppercasePassword = password.ToUpper();
            return invitationStorage.Query(_ => _.Any(x => x.Token == uppercaseToken && x.Assignment.Password == uppercasePassword));
        }

        public void MigrateInvitationToNewAssignment(int oldAssignmentId, int newAssignmentId)
        {
            var invitation = invitationStorage.Query(_ => _.SingleOrDefault(x => x.AssignmentId == oldAssignmentId && x.InterviewId == null));
            if (invitation != null)
            {
                invitation.UpdateAssignmentId(newAssignmentId);
                invitationStorage.Store(invitation, invitation.Id);
            }
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

        private static Expression<Func<Invitation, bool>> LastReminderIsExpired(DateTime thresholdDate)
        {
            return x => x.LastReminderSentOnUtc != null &&
                        x.LastReminderSentOnUtc < thresholdDate;
        }

        private static Expression<Func<Invitation, bool>> NoReminderAndInvitationIsExpired(DateTime thresholdDate)
        {
            return x => x.LastReminderSentOnUtc == null &&
                        x.SentOnUtc != null &&
                        x.SentOnUtc < thresholdDate;
        }

        private static Expression<Func<Invitation, bool>> HasInterview()
        {
            return x => x.InterviewId != null;
        }
        
        private static Expression<Func<Invitation, bool>> HasNoInterview()
        {
            return x => x.InterviewId == null;
        }

        private static Expression<Func<Invitation, bool>> NotArchived()
        {
            return x => x.Assignment.Archived == false;
        }

        private static Expression<Func<Invitation, bool>> HasEmail()
        {
            return x => x.Assignment.Email != null && x.Assignment.Email != string.Empty;
        }

        private static Expression<Func<Invitation, bool>> FilteredByQuestionnaire(QuestionnaireIdentity questionnaireIdentity)
        {
            return x =>
                x.Assignment.QuestionnaireId.QuestionnaireId == questionnaireIdentity.QuestionnaireId &&
                x.Assignment.QuestionnaireId.Version == questionnaireIdentity.Version;
        }

    }

    public interface IInvitationService
    {
        void CreateInvitationForWebInterview(Assignment assignment);
        int CreateInvitationForPublicLink(Assignment assignment, string interviewId);

        int GetCountOfInvitations(QuestionnaireIdentity questionnaireIdentity);
        int GetCountOfNotSentInvitations(QuestionnaireIdentity questionnaireIdentity);
        int GetCountOfSentInvitations(QuestionnaireIdentity questionnaireIdentity);
        InvitationDistributionStatus GetEmailDistributionStatus();
        List<int> GetInvitationIdsToSend(QuestionnaireIdentity questionnaireIdentity);
        Invitation GetInvitation(int invitationId);
        void InvitationWasNotSent(int invitationId, int assignmentId, string email, string reason);
        void MarkInvitationAsSent(int invitationId, string emailId);
        void MarkInvitationAsReminded(int invitationId, string emailId);

        void RequestEmailDistributionProcess(QuestionnaireIdentity questionnaireIdentity, string identityName, string questionnaireTitle);

        void StartEmailDistribution();
        void CompleteEmailDistribution();
        void EmailDistributionFailed();
        void EmailDistributionCanceled();
        void CancelEmailDistribution();
        CancellationToken GetCancellationToken();
        IEnumerable<int> GetPartialResponseInvitations(QuestionnaireIdentity identity, int thresholdDays);
        IEnumerable<int> GetNoResponseInvitations(QuestionnaireIdentity identity, int thresholdDays);
        void ReminderWasNotSent(int invitationId, int assignmentId, string address, string message);
        List<Invitation> GetInvitationsToExport(QuestionnaireIdentity questionnaireIdentity);
        IEnumerable<QuestionnaireLiteViewItem> GetQuestionnairesWithInvitations();
        Invitation GetInvitationByToken(string token);
        Invitation GetInvitationByTokenAndPassword(string token, string password);
        Invitation GetInvitationByAssignmentId(int assignmentId);
        void InterviewWasCreated(int invitationId, string interviewId);
        bool IsValidTokenAndPassword(string token, string password);
        void MigrateInvitationToNewAssignment(int oldAssignmentId, int newAssignmentId);
    }
}
