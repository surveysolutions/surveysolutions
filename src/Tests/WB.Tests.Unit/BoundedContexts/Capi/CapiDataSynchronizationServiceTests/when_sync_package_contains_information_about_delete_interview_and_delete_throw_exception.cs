using System;
using Machine.Specifications;
using Moq;
using WB.Core.BoundedContexts.Capi.ChangeLog;
using WB.Core.BoundedContexts.Capi.Implementation.Services;
using WB.Core.BoundedContexts.Capi.Services;
using WB.Core.BoundedContexts.Capi.Views.InterviewMetaInfo;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.Infrastructure.ReadSide;
using WB.Core.SharedKernel.Structures.Synchronization;
using WB.Core.SharedKernel.Structures.Synchronization.SurveyManagement;
using WB.Core.SharedKernels.DataCollection.Repositories;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.BoundedContexts.Capi.CapiDataSynchronizationServiceTests
{
    internal class when_sync_package_contains_information_about_delete_interview_and_delete_throw_exception : CapiDataSynchronizationServiceTestContext
    {
        Establish context = () =>
        {
            interviewId = Guid.NewGuid();
            responsibleId = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");

            syncItem = new InterviewSyncPackageDto
                       {
                           Content = interviewId.ToString(), 
                           MetaInfo = "some metadata", 
                       };

            commandService = new Mock<ICommandService>();

            plainQuestionnaireRepositoryMock = new Mock<IPlainQuestionnaireRepository>();

            changeLogManipulator = new Mock<IChangeLogManipulator>();

            cleanUpExecutorMock = new Mock<ICapiCleanUpService>();
            cleanUpExecutorMock.Setup(x => x.DeleteInterview(interviewId)).Throws<NullReferenceException>();

            var viewFactory = new Mock<IViewFactory<InterviewMetaInfoInputModel, InterviewMetaInfo>>();
            var existingInterview = new InterviewMetaInfo
            {
                ResponsibleId = responsibleId
            };
            viewFactory.SetReturnsDefault(existingInterview);

            capiDataSynchronizationService = CreateCapiDataSynchronizationService(changeLogManipulator.Object, 
                commandService.Object, null, null,
                plainQuestionnaireRepositoryMock.Object, null, cleanUpExecutorMock.Object,
                interviewMetaInfoFactory: viewFactory.Object);
        };

        Because of = () => exception = Catch.Exception(() => capiDataSynchronizationService.ProcessDownloadedPackage(syncItem, SyncItemType.DeleteInterview, responsibleId));

        It should_never_call_any_command =
            () => commandService.Verify(x => x.Execute(Moq.It.IsAny<ICommand>(), null), Times.Never);

        It should_cleanup_data_for_interview =
            () => cleanUpExecutorMock.Verify(x => x.DeleteInterview(interviewId), Times.Once);

        It should_not_throw_Exception = () => 
            exception.ShouldBeNull();

        private static CapiDataSynchronizationService capiDataSynchronizationService;
        private static InterviewSyncPackageDto syncItem;
        private static Mock<ICommandService> commandService;
        private static Mock<IPlainQuestionnaireRepository> plainQuestionnaireRepositoryMock;
        private static Mock<IChangeLogManipulator> changeLogManipulator;
        private static Mock<ICapiCleanUpService> cleanUpExecutorMock;
        private static Guid interviewId;
        private static Exception exception;
        private static Guid responsibleId;
    }
}
