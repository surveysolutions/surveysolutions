using System.Linq;
using NUnit.Framework;
using WB.Core.BoundedContexts.Headquarters.DataExport.Views;
using WB.Core.BoundedContexts.Headquarters.Invitations;
using WB.ServicesIntegration.Export;
using WB.Tests.Abc;
using InterviewStatus = WB.Core.SharedKernels.DataCollection.ValueObjects.Interview.InterviewStatus;

namespace WB.Tests.Unit.BoundedContexts.Headquarters.Invitations
{
    [TestOf(nameof(InvitationService.GetInvitationsWithRejectedInterview))]
    public class InvitationService_GetInvitationsWithRejectedInterview
    {
        [Test]
        public void should_return_invitation_with_non_sent_rejection_email()
        {
            var questionnaireId = Create.Entity.QuestionnaireIdentity();
            
            var interview = Create.Entity.InterviewSummary(status: InterviewStatus.RejectedBySupervisor);
            interview.InterviewCommentedStatuses.Add(Create.Entity.InterviewCommentedStatus(status: InterviewExportedAction.RejectedBySupervisor,
                interviewSummary: interview));
            
            var invitation = Create.Entity.Invitation(1, 
                Create.Entity.Assignment(questionnaireIdentity: questionnaireId),
                interview: interview);
            var invitationService = Create.Service.InvitationService(invitation);
            
            // Act
            var invitations = invitationService.GetInvitationsWithRejectedInterview(questionnaireId)
                .ToList();
            
            // assert
            Assert.That(invitations, Has.Count.EqualTo(1));
            Assert.That(invitations.First(), Is.EqualTo(1));
        }
        
        [Test]
        public void should_not_return_invitation_with_sent_rejection_email()
        {
            var questionnaireId = Create.Entity.QuestionnaireIdentity();
            
            var interview = Create.Entity.InterviewSummary(status: InterviewStatus.RejectedBySupervisor);
            interview.InterviewCommentedStatuses.Add(Create.Entity.InterviewCommentedStatus(status: InterviewExportedAction.RejectedBySupervisor,
                interviewSummary: interview));
            
            var invitation = Create.Entity.Invitation(1, 
                Create.Entity.Assignment(questionnaireIdentity: questionnaireId),
                interview: interview);
            var invitationService = Create.Service.InvitationService(invitation);
            
            invitationService.MarkRejectedInterviewReminderSent(invitation.Id, "email");
            
            // Act
            var invitations = invitationService.GetInvitationsWithRejectedInterview(questionnaireId);
            
            // assert
            Assert.That(invitations, Is.Empty);
        }
    }
}
