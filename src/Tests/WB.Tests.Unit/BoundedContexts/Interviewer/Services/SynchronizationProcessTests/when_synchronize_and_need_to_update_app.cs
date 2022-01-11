using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Moq;
using NUnit.Framework;
using WB.Core.BoundedContexts.Interviewer.Implementation.Services;
using WB.Core.BoundedContexts.Interviewer.Services;
using WB.Core.BoundedContexts.Interviewer.Views;
using WB.Core.GenericSubdomains.Portable.Implementation;
using WB.Core.GenericSubdomains.Portable.ServiceLocation;
using WB.Core.Infrastructure.HttpServices.HttpClient;
using WB.Core.SharedKernels.DataCollection.WebApi;
using WB.Core.SharedKernels.Enumerator.Implementation.Services;
using WB.Core.SharedKernels.Enumerator.Implementation.Services.Synchronization;
using WB.Core.SharedKernels.Enumerator.Services.Synchronization;
using WB.Core.SharedKernels.Enumerator.Services.Workspace;
using WB.Core.SharedKernels.Enumerator.Views;
using WB.Tests.Abc;

namespace WB.Tests.Unit.BoundedContexts.Interviewer.Services.SynchronizationProcessTests
{
    [NUnit.Framework.TestOf(typeof(InterviewerOnlineSynchronizationProcess))]
    internal class when_synchronize_and_need_to_update_app
    {
        [Test]
        public async Task should_try_to_download_last_version()
        {
            var interviewerIdentity = new InterviewerIdentity() { Name = "name", Token = "token", Workspace = "primary"};

            Mock<IOnlineSynchronizationService> synchronizationServiceMock = new Mock<IOnlineSynchronizationService>();

            var principalMock = Mock.Get(SetUp.InterviewerPrincipal(interviewerIdentity));
            
            synchronizationServiceMock
                .Setup(x => x.CanSynchronizeAsync(It.IsAny<RestCredentials>(), null, It.IsAny<CancellationToken>()))
                .Throws(new SynchronizationException(type: SynchronizationExceptionType.UpgradeRequired, message: "Test UpgradeRequired", innerException: null));
            synchronizationServiceMock
                .Setup(x => x.GetInterviewerAsync(It.IsAny<RestCredentials>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new InterviewerApiView() { Workspaces = new List<UserWorkspaceApiView>() { new UserWorkspaceApiView() { Name = "primary"}}});

            var workspaceService = Mock.Of<IWorkspaceService>(w =>
                w.GetAll() == new[] {new WorkspaceView() {Id = "primary"}});
            
            var updateApplicationMock = new Mock<IUpdateApplicationSynchronizationStep>();

            var serviceLocator = Mock.Of<IServiceLocator>(sl => sl.GetInstance<IUpdateApplicationSynchronizationStep>() == updateApplicationMock.Object);

            var viewModel = Create.Service.SynchronizationProcess(principal: principalMock.Object,
                synchronizationService: synchronizationServiceMock.Object,
                serviceLocator: serviceLocator,
                workspaceService: workspaceService);

            // Act
            await viewModel.SynchronizeAsync(new Progress<SyncProgressInfo>(), CancellationToken.None);

            // Assert
            updateApplicationMock.Verify(x => x.ExecuteAsync(), Times.Once());
        }
    }
}
