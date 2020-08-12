using Moq;
using NUnit.Framework;
using WB.Core.BoundedContexts.Headquarters.Invitations;
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
    }
}