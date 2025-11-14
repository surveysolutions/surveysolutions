using System;
using System.Collections.Generic;
using System.Threading;
using WB.Core.BoundedContexts.Headquarters.Assignments;
using WB.Core.BoundedContexts.Headquarters.Views.Questionnaire;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;

namespace WB.Core.BoundedContexts.Headquarters.Invitations
{
    public interface IInvitationService
    {
        void CreateInvitationForWebInterview(Assignment assignment);
        int GetOrCreateInvitationForPublicLink(Assignment assignment, string interviewId);
        Invitation GetInvitationForPublicLinkOrNull(Assignment assignment, string interviewId);

        int GetCountOfInvitations(QuestionnaireIdentity questionnaireIdentity);
        int GetCountOfNotSentInvitations(QuestionnaireIdentity questionnaireIdentity);
        int GetCountOfSentInvitations(QuestionnaireIdentity questionnaireIdentity);
        InvitationDistributionStatus GetEmailDistributionStatus();
        List<int> GetInvitationIdsToSend(QuestionnaireIdentity questionnaireIdentity);
        Invitation GetInvitation(int invitationId);
        void MarkInvitationAsSent(int invitationId, string emailId);
        void MarkInvitationAsReminded(int invitationId, string emailId);

        void RequestEmailDistributionProcess(QuestionnaireIdentity questionnaireIdentity, string identityName, string questionnaireTitle);

        InvitationDistributionStatus StartEmailDistribution();
        void CompleteEmailDistribution(InvitationDistributionStatus status);
        void EmailDistributionFailed(InvitationDistributionStatus status);
        void EmailDistributionCanceled(InvitationDistributionStatus status);
        void EmailDistributionIncrementProcessedCount(InvitationDistributionStatus status);
        void EmailDistributionIncrementErrorCount(InvitationDistributionStatus status, int invitationId, int assignmentId, string email, string reason);
        void CancelEmailDistribution();
        CancellationToken GetCancellationToken();
        IEnumerable<int> GetPartialResponseInvitations(QuestionnaireIdentity identity, int thresholdDays);
        IEnumerable<int> GetNoResponseInvitations(QuestionnaireIdentity identity, int thresholdDays);
        void ReminderWasNotSent(int invitationId, int assignmentId, string address, string message);
        List<Invitation> GetInvitationsToExport(QuestionnaireIdentity questionnaireIdentity);
        IEnumerable<QuestionnaireBrowseItem> GetQuestionnairesWithInvitations();
        Invitation GetInvitationByToken(string token);
        Invitation GetInvitationByTokenAndPassword(string token, string password);
        Invitation GetInvitationByAssignmentId(int assignmentId);
        void InterviewWasCreated(int invitationId, string interviewId);
        bool IsValidTokenAndPassword(string token, string password);
        void MigrateInvitationToNewAssignment(int oldAssignmentId, int newAssignmentId);
        IEnumerable<int> GetInvitationsWithRejectedInterview(QuestionnaireIdentity questionnaireIdentity);
        void MarkRejectedInterviewReminderSent(int invitationId, string emailId);
        void RejectedInterviewReminderWasNotSent(int invitationId);
    }
}
