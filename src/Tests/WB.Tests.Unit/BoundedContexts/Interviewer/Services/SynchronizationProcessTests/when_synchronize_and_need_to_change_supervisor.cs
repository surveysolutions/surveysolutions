using System;
using System.Threading;
using Moq;
using NUnit.Framework;
using WB.Core.BoundedContexts.Interviewer.Implementation.Services;
using WB.Core.BoundedContexts.Interviewer.Services;
using WB.Core.BoundedContexts.Interviewer.Services.Infrastructure;
using WB.Core.BoundedContexts.Interviewer.Views;
using WB.Core.GenericSubdomains.Portable.Implementation;
using WB.Core.GenericSubdomains.Portable.Tasks;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure.Storage;
using WB.Core.SharedKernels.Enumerator.Services.Synchronization;
using WB.Tests.Abc;

namespace WB.Tests.Unit.BoundedContexts.Interviewer.Services.SynchronizationProcessTests
{
    [TestFixture]
    public class when_synchronize_and_need_to_change_supervisor
    {
        [OneTimeSetUp]
        public void Context()
        {
            var interviewerIdentity = new InterviewerIdentity {Name = "name", PasswordHash = "hash", Token = "Outdated token", SupervisorId = Id.g1};

            PrincipalMock = Mock.Get(Setup.InterviewerPrincipal(interviewerIdentity));
          
            SynchronizationServiceMock
                .Setup(x => x.GetCurrentSupervisor(It.IsAny<CancellationToken>(), It.IsAny<RestCredentials>()))
                .ReturnsAsync(Id.gA);

            InterviewerStorageMock.Setup(x => x.FirstOrDefault()).Returns(interviewerIdentity);

            viewModel = Create.Service.SynchronizationProcess(principal: PrincipalMock.Object,
                synchronizationService: SynchronizationServiceMock.Object,
                interviewersPlainStorage: InterviewerStorageMock.Object);

            viewModel
                .SynchronizeAsync(new Progress<SyncProgressInfo>(), CancellationToken.None)
                .WaitAndUnwrapException();
        }

        [Test]
        public void should_store_updated_supervisor_id_in_plain_storage()
        {
            InterviewerStorageMock.Verify(
                x => x.Store(It.Is<InterviewerIdentity>(i => i.SupervisorId == Id.gA)), Times.Once);

            PrincipalMock.Verify(x => x.SignInWithHash("name", "hash", true), Times.Once);
        }

        static InterviewerOnlineSynchronizationProcess viewModel;
        static readonly Mock<IPlainStorage<InterviewerIdentity>> InterviewerStorageMock = new Mock<IPlainStorage<InterviewerIdentity>>();
        static Mock<IInterviewerPrincipal> PrincipalMock = new Mock<IInterviewerPrincipal>();
        static readonly Mock<IOnlineSynchronizationService>  SynchronizationServiceMock = new Mock<IOnlineSynchronizationService>();
    }
}
