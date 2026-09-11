using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Moq;
using NUnit.Framework;
using WB.Core.BoundedContexts.Interviewer.Implementation.Services;
using WB.Core.BoundedContexts.Interviewer.Services;
using WB.Core.BoundedContexts.Interviewer.Services.Infrastructure;
using WB.Core.BoundedContexts.Interviewer.Views;
using WB.Core.GenericSubdomains.Portable.Implementation;
using WB.Core.GenericSubdomains.Portable.ServiceLocation;
using WB.Core.Infrastructure.HttpServices.HttpClient;
using WB.Core.SharedKernels.DataCollection.WebApi;
using WB.Core.SharedKernels.Enumerator.Implementation.Services.Synchronization;
using WB.Core.SharedKernels.Enumerator.Services.Synchronization;
using WB.Core.SharedKernels.Enumerator.Services.Workspace;
using WB.Core.SharedKernels.Enumerator.Views;
using WB.Tests.Abc;

namespace WB.Tests.Unit.BoundedContexts.Interviewer.Services.SynchronizationProcessTests
{
    [TestOf(typeof(InterviewerOnlineSynchronizationProcess))]
    internal class when_synchronize_in_online_mode
    {
        [Test]
        public async Task should_check_server_version_before_executing_synchronization_steps()
        {
            var interviewerIdentity = new InterviewerIdentity
            {
                Name = "name",
                Token = "token",
                SupervisorId = Id.g1,
                Workspace = "primary"
            };

            var principalMock = Mock.Get(SetUp.InterviewerPrincipal(interviewerIdentity));
            var synchronizationServiceMock = new Mock<IOnlineSynchronizationService>();
            synchronizationServiceMock
                .Setup(x => x.CanSynchronizeAsync(It.IsAny<RestCredentials>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);
            synchronizationServiceMock
                .Setup(x => x.GetInterviewerAsync(It.IsAny<RestCredentials>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new InterviewerApiView
                {
                    Workspaces = new List<UserWorkspaceApiView>
                    {
                        new UserWorkspaceApiView { Name = "primary" }
                    }
                });
            synchronizationServiceMock
                .Setup(x => x.GetCurrentSupervisor(It.IsAny<RestCredentials>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(Id.g1);

            var workspaceService = Mock.Of<IWorkspaceService>(w =>
                w.GetAll() == new[] { new WorkspaceView { Id = "primary" } });

            var updateApplicationStep = new Mock<IUpdateApplicationSynchronizationStep>(MockBehavior.Strict);
            var synchronizationStep = new Mock<ISynchronizationStep>(MockBehavior.Strict);
            synchronizationStep.SetupGet(x => x.SortOrder).Returns(1);
            synchronizationStep.SetupProperty(x => x.Context);

            var callSequence = new MockSequence();
            updateApplicationStep.InSequence(callSequence)
                .Setup(x => x.CheckServerVersionAsync(It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);
            synchronizationStep.InSequence(callSequence)
                .Setup(x => x.ExecuteAsync())
                .Returns(Task.CompletedTask);

            var serviceLocator = new Mock<IServiceLocator>();
            serviceLocator
                .Setup(x => x.GetInstance<IUpdateApplicationSynchronizationStep>())
                .Returns(updateApplicationStep.Object);
            serviceLocator
                .Setup(x => x.GetAllInstances<ISynchronizationStep>())
                .Returns(new[] { synchronizationStep.Object });

            var synchronizationProcess = Create.Service.SynchronizationProcess(
                principal: principalMock.Object,
                synchronizationService: synchronizationServiceMock.Object,
                workspaceService: workspaceService,
                serviceLocator: serviceLocator.Object);

            await synchronizationProcess.SynchronizeAsync(new Progress<SyncProgressInfo>(), CancellationToken.None);

            updateApplicationStep.Verify(x => x.CheckServerVersionAsync(It.IsAny<CancellationToken>()), Times.Once);
            synchronizationStep.Verify(x => x.ExecuteAsync(), Times.Once);
        }
    }
}
