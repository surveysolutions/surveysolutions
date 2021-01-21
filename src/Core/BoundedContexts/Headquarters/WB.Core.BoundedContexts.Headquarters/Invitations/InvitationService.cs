using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using WB.Core.BoundedContexts.Headquarters.Assignments;
using WB.Core.BoundedContexts.Headquarters.DataExport.Views;
using WB.Core.BoundedContexts.Headquarters.Views;
using WB.Core.BoundedContexts.Headquarters.Views.Questionnaire;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.Infrastructure.Services;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;

namespace WB.Core.BoundedContexts.Headquarters.Invitations
{
    public class InvitationService : IInvitationService
    {
        private readonly ITokenGenerator tokenGenerator;
        private readonly IPlainStorageAccessor<Invitation> invitationStorage;
        private readonly IPlainKeyValueStorage<InvitationDistributionStatus> invitationsDistributionStatusStorage;
        private readonly IAggregateRootPrototypePromoterService promoterService;
        private static CancellationTokenSource cancellationTokenSource;

        public InvitationService(
            IPlainStorageAccessor<Invitation> invitationStorage,
            IPlainKeyValueStorage<InvitationDistributionStatus> invitationsDistributionStatusStorage, 
            IAggregateRootPrototypePromoterService promoterService,
            ITokenGenerator tokenGenerator)
        {
            this.invitationStorage = invitationStorage;
            this.invitationsDistributionStatusStorage = invitationsDistributionStatusStorage;
            this.promoterService = promoterService;
            this.tokenGenerator = tokenGenerator;
        }
        
        public int CreateInvitationForPublicLink(Assignment assignment, string interviewId)
        {
            var invitation = new Invitation(assignment.Id);
            var token = tokenGenerator.GenerateUnique();
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

            var assignmentId = assignment.Id;
            var invitation = new Invitation(assignmentId);

            if (assignment.InPrivateWebMode())
            {
                /*
                Quantity  Password 	    Email 	    
                1 	      not empty 	empty      Public link, unique passwords. Token should be unique for all assignments
                1         empty 	    not empty  Private link, no password
                1 	      not empty 	not empty  Private link, with password
                */
                if (hasEmail)
                {
                    var token = tokenGenerator.GenerateUnique();
                    invitation.SetToken(token);
                }
                else
                {
                    var token = tokenGenerator.Generate(assignment.QuestionnaireId);
                    invitation.SetToken(token, TokenKind.AssignmentResolvedByPassword);
                }
            }
            else
            {
                /*
                Quantity  Password 	    Email 	    
                -1 	      empty 	    empty      Public link, no password
                -1 	      not empty 	empty      Public link, with password
                */
                var token = tokenGenerator.GenerateUnique();
                invitation.SetToken(token);
            }

            invitationStorage.Store(invitation, invitation.Id);
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

        public IEnumerable<int> GetInvitationsWithRejectedInterview(QuestionnaireIdentity questionnaireIdentity)
        {
            var list = invitationStorage.Query(_ => _
                .Where(FilteredByQuestionnaire(questionnaireIdentity))
                .Where(HasRejectedInterview())
                .Where(NoRejectedInterviewEmailSent())
                .Select(x => x.Id)
                .ToList());
            return list;
        }

        public void MarkRejectedInterviewReminderSent(int invitationId, string emailId)
        {
            var invitation = this.invitationStorage.GetById(invitationId);
            var lastRejectedStatus =
                invitation.Interview.InterviewCommentedStatuses.Last(x =>
                    x.Status == InterviewExportedAction.RejectedBySupervisor);
            invitation.RejectedReminderSent(emailId, lastRejectedStatus.Position);
        }

        public void RejectedInterviewReminderWasNotSent(int invitationId)
        {
            var invitation = this.invitationStorage.GetById(invitationId);
            invitation.RejectedReminderWasNotSent();
        }

        private Expression<Func<Invitation,bool>> NoRejectedInterviewEmailSent()
        {
            return x => 
                        x.Interview.InterviewCommentedStatuses
                                   .Any(s => s.Status == InterviewExportedAction.RejectedBySupervisor &&
                                             (x.LastRejectedStatusPosition == null || s.Position > x.LastRejectedStatusPosition));
        }

        private Expression<Func<Invitation,bool>> HasRejectedInterview()
        {
            return x => x.Interview.Status == InterviewStatus.RejectedBySupervisor;
        }

        public List<Invitation> GetInvitationsToExport(QuestionnaireIdentity questionnaireIdentity)
        {
            return invitationStorage.Query(_ => _
                .Where(FilteredByQuestionnaire(questionnaireIdentity))
                .Where(NotArchived())
                .Where(HasNoInterview())
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

        public IEnumerable<QuestionnaireBrowseItem> GetQuestionnairesWithInvitations()
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
            return invitationStorage.Query(_ => _.SingleOrDefault(x => x.AssignmentId == assignmentId && x.InterviewId == null));
        }

        public void InterviewWasCreated(int invitationId, string interviewId)
        {
            var invitation = this.GetInvitation(invitationId);
            invitation.InterviewWasCreated(interviewId);
            invitationStorage.Store(invitation, invitationId);
            promoterService.MaterializePrototypeIfRequired(Guid.Parse(interviewId));
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
            if (status != null)
            {
                status.ProcessedCount++;
                this.invitationsDistributionStatusStorage.Store(status, AppSetting.InvitationsDistributionStatus);
            }
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
}
