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
        [Test]
        public void should_promote_interview()
        {
            var questionnaireIdentity = Create.Entity.QuestionnaireIdentity();
            var invitations = new InMemoryPlainStorageAccessor<Invitation>();
            var promoter = new Mock<IAggregateRootPrototypePromoterService>();
            
            var service = Create.Service.InvitationService(invitations, promoter: promoter.Object);
            var assignment = Create.Entity.Assignment(questionnaireIdentity: questionnaireIdentity, webMode: true, quantity: 1, password: "yes");
            invitations.Store(Create.Entity.Invitation(1, assignment), 1);

            service.InterviewWasCreated(1, Id.g1.FormatGuid());
            
            promoter.Verify(p => p.MaterializePrototypeIfRequired(Id.g1), Times.Once);
        }

        [Test]
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
