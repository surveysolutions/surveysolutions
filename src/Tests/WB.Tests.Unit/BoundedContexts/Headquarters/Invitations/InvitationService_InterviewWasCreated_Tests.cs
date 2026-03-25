using System.Collections.Generic;
using System.Threading.Tasks;
using Moq;
using NUnit.Framework;
using WB.Core.BoundedContexts.Headquarters.Invitations;
using WB.Core.BoundedContexts.Headquarters.Views.Interview;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.Infrastructure.Implementation;
using WB.Core.Infrastructure.Services;
using WB.Tests.Abc;

namespace WB.Tests.Unit.BoundedContexts.Headquarters.Invitations
{
    [TestOf(typeof(InvitationService))]
    public class InvitationService_InterviewWasCreated_Tests
    {
        [Test(Description = "KP-14642")]
        public void should_not_send_invitation_for_completed_assignment()
        {
            var questionnaireIdentity = Create.Entity.QuestionnaireIdentity();
            var invitations = new InMemoryPlainStorageAccessor<Invitation>();

            var assignment = Create.Entity.Assignment(questionnaireIdentity: questionnaireIdentity, 
                webMode: true, 
                quantity: 1,
                email: "test@test.com",
                interviewSummary: new [] {Create.Entity.InterviewSummary() });
            invitations.Store(Create.Entity.Invitation(1, assignment), 1);

            var service = Create.Service.InvitationService(invitations);
            
            // act
            var invitationIdsToSend = service.GetInvitationIdsToSend(questionnaireIdentity);

            // Assert
            Assert.That(invitationIdsToSend, Is.Empty);
        }
    }
}
