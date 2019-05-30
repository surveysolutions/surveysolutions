using System.Linq;
using NUnit.Framework;
using WB.Core.BoundedContexts.Headquarters.Invitations;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.Infrastructure.Implementation;
using WB.Tests.Abc;

namespace WB.Tests.Unit.BoundedContexts.Headquarters.Invitations
{
    [TestOf(typeof(InvitationService))]
    public class InvitationService_CreateInvitationForWebInterview_Tests
    {
        [Test]
        public void should_generate_token_for_assignment_with_email_and_in_web_mode()
        {
            var invitations = new InMemoryPlainStorageAccessor<Invitation>();
            var service = Create.Service.InvitationService(invitations);

            service.CreateInvitationForWebInterview(
                Create.Entity.Assignment(id: 5, 
                    questionnaireIdentity: Create.Entity.QuestionnaireIdentity(),
                    email: "test@test.com",
                    password: "password",
                    quantity: 1));

            var invitation = invitations.Query(_ => _.FirstOrDefault());
            Assert.That(invitation, Is.Not.Null, "Invitation should be saved");
            Assert.That(invitation, Has.Property(nameof(Invitation.Token)).Not.Null.Or.Empty);
        }
    }

    [TestOf(typeof(InvitationService))]
    public class InvitationService_GetInvitationsToExport_Tests
    {
        [Test(Description = "KP-12875")]
        public void should_not_include_invitations_with_interviews()
        {
            var questionnaireIdentity = Create.Entity.QuestionnaireIdentity();
            var assignment = Create.Entity.Assignment(questionnaireIdentity: questionnaireIdentity);
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
