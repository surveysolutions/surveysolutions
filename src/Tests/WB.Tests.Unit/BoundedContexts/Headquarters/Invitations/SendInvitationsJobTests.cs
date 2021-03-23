using System.Threading.Tasks;
using Moq;
using NUnit.Framework;
using Quartz;
using WB.Core.BoundedContexts.Headquarters.EmailProviders;
using WB.Core.BoundedContexts.Headquarters.Invitations;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Tests.Abc;

namespace WB.Tests.Unit.BoundedContexts.Headquarters.Invitations
{
    [TestOf(typeof(SendInvitationsJob))]
    public class SendInvitationsJobTests
    {
        [Test]
        public async Task when_email_service_is_not_configured()
        {
            var invitationServiceMock = new Mock<IInvitationService>();
            var emailServiceMock = new Mock<IEmailService>();
            emailServiceMock.Setup(x => x.IsConfigured()).Returns(false);

            //arrange 
            var job = Create.Service.SendInvitationsJob(
                invitationService: invitationServiceMock.Object, 
                emailService: emailServiceMock.Object);

            //act
            await job.Execute(Mock.Of<IJobExecutionContext>());

            //assert
            invitationServiceMock.Verify(x => x.GetInvitationIdsToSend(It.IsAny<QuestionnaireIdentity>()), Times.Never);
        }
    }
}
