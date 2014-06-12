using System;
using Machine.Specifications;
using Moq;
using Ncqrs.Commanding;
using Ncqrs.Commanding.ServiceModel;
using WB.Core.BoundedContexts.Capi.Synchronization.ChangeLog;
using WB.Core.BoundedContexts.Capi.Synchronization.Cleaner;
using WB.Core.BoundedContexts.Capi.Synchronization.Implementation;
using WB.Core.SharedKernel.Structures.Synchronization;
using WB.Core.SharedKernels.DataCollection.Repositories;
using It = Machine.Specifications.It;

namespace WB.Core.BoundedContext.Capi.Synchronization.Tests.DataProcessorTests
{
    internal class when_sync_package_contains_information_about_delete_interview_and_delete_throw_exception : DataProcessorTestContext
    {
        Establish context = () =>
        {
            interviewId = Guid.NewGuid();

            syncItem = new SyncItem() { ItemType = SyncItemType.DeleteQuestionnare, IsCompressed = false, Content = interviewId.ToString(), MetaInfo = "some metadata", Id = Guid.NewGuid() };

            commandService = new Mock<ICommandService>();

            plainQuestionnaireRepositoryMock = new Mock<IPlainQuestionnaireRepository>();

            changeLogManipulator = new Mock<IChangeLogManipulator>();

            cleanUpExecutorMock = new Mock<ICleanUpExecutor>();
            cleanUpExecutorMock.Setup(x => x.DeleteInterveiw(interviewId)).Throws<NullReferenceException>();

            dataProcessor = CreateDataProcessor(changeLogManipulator.Object, commandService.Object, null, null,
                plainQuestionnaireRepositoryMock.Object, null, cleanUpExecutorMock.Object);
        };

        Because of = () => exception = Catch.Exception(() =>dataProcessor.ProcessPulledItem(syncItem));

        It should_never_call_any_command =
            () =>
                commandService.Verify(
                    x =>
                        x.Execute(
                            Moq.It.IsAny<ICommand>(), null),
                    Times.Never);

        It should_cleanup_data_for_interview =
            () =>
                cleanUpExecutorMock.Verify(
                    x => x.DeleteInterveiw(interviewId),
                    Times.Once);

        It should_not_create_public_record_in_change_log_for_sync_item =
        () =>
            changeLogManipulator.Verify(
                x =>
                    x.CreatePublicRecord(syncItem.Id),
                Times.Never);

        It should_throw_NullReferenceException = () =>
            exception.ShouldBeOfType<NullReferenceException>();

        private static DataProcessor dataProcessor;
        private static SyncItem syncItem;
        private static Mock<ICommandService> commandService;
        private static Mock<IPlainQuestionnaireRepository> plainQuestionnaireRepositoryMock;
        private static Mock<IChangeLogManipulator> changeLogManipulator;
        private static Mock<ICleanUpExecutor> cleanUpExecutorMock;
        private static Guid interviewId;
        private static Exception exception;
    }
}
