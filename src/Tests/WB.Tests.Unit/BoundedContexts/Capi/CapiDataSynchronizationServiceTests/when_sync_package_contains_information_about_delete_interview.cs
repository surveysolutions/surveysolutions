using System;
using Machine.Specifications;
using Moq;
using WB.Core.BoundedContexts.Capi.ChangeLog;
using WB.Core.BoundedContexts.Capi.Implementation.Services;
using WB.Core.BoundedContexts.Capi.Services;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.SharedKernel.Structures.Synchronization;
using WB.Core.SharedKernel.Structures.Synchronization.SurveyManagement;
using WB.Core.SharedKernels.DataCollection.Repositories;

using It = Machine.Specifications.It;

namespace WB.Tests.Unit.BoundedContexts.Capi.CapiDataSynchronizationServiceTests
{
    internal class when_sync_package_contains_information_about_delete_interview : CapiDataSynchronizationServiceTestContext
    {
        Establish context = () =>
        {
            interviewId = Guid.NewGuid();

            syncItem = new InterviewSyncPackageDto
                       {
                           Content = interviewId.ToString(), MetaInfo = "some metadata"
                       };

            commandService = new Mock<ICommandService>();

            plainQuestionnaireRepositoryMock = new Mock<IPlainQuestionnaireRepository>();

            changeLogManipulator = new Mock<IChangeLogManipulator>();

            cleanUpExecutorMock=new Mock<ICapiCleanUpService>();

            capiDataSynchronizationService = CreateCapiDataSynchronizationService(changeLogManipulator.Object, commandService.Object, null, null,
                plainQuestionnaireRepositoryMock.Object, null, cleanUpExecutorMock.Object);
        };

        Because of = () => capiDataSynchronizationService.ProcessDownloadedPackage(syncItem, SyncItemType.DeleteInterview);

        It should_never_call_any_command =
            () => commandService.Verify(x => x.Execute(Moq.It.IsAny<ICommand>(), null), Times.Never);

        It should_cleanup_data_for_interview =
            () => cleanUpExecutorMock.Verify(x => x.DeleteInterview(interviewId), Times.Once);

        private static CapiDataSynchronizationService capiDataSynchronizationService;
        private static InterviewSyncPackageDto syncItem;
        private static Mock<ICommandService> commandService;
        private static Mock<IPlainQuestionnaireRepository> plainQuestionnaireRepositoryMock;
        private static Mock<IChangeLogManipulator> changeLogManipulator;
        private static Mock<ICapiCleanUpService> cleanUpExecutorMock;
        private static Guid interviewId;
    }
}
