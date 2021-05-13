using System.Linq;
using NUnit.Framework;
using WB.Core.BoundedContexts.Headquarters.Invitations;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.Infrastructure.Implementation;
using WB.Tests.Abc;

namespace WB.Tests.Unit.BoundedContexts.Headquarters.Invitations
{
    [TestOf(typeof(InvitationService))]
    public class InvitationService_GetInvitationsToExport_Tests
    {
        [Test(Description = "KP-12875")]
        public void should_not_include_invitations_with_interviews()
        {
            var questionnaireIdentity = Create.Entity.QuestionnaireIdentity();
            var assignment = Create.Entity.Assignment(questionnaireIdentity: questionnaireIdentity, webMode:true);
            var invitations = new InMemoryPlainStorageAccessor<Invitation>();
            invitations.Store(Create.Entity.Invitation(1, assignment), 1);
            invitations.Store(Create.Entity.Invitation(2, assignment, interviewId: Id.gA.FormatGuid()), 2);

            var service = Create.Service.InvitationService(invitations);

            // Act
            var invitationsToExport = service.GetInvitationsToExport(questionnaireIdentity);

            Assert.That(invitationsToExport, Has.Count.EqualTo(1));
            Assert.That(invitationsToExport.Single(), Has.Property(nameof(Invitation.Id)).EqualTo(1));
        }
    }
}
