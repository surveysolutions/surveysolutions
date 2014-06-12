using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Machine.Specifications;
using Main.Core.Documents;
using Moq;
using Ncqrs.Commanding;
using Ncqrs.Commanding.ServiceModel;
using WB.Core.BoundedContexts.Capi.Synchronization.Synchronization.ChangeLog;
using WB.Core.BoundedContexts.Capi.Synchronization.Synchronization.Cleaner;
using WB.Core.BoundedContexts.Capi.Synchronization.Synchronization.Pull;
using WB.Core.SharedKernel.Structures.Synchronization;
using WB.Core.SharedKernel.Utils.Serialization;
using WB.Core.SharedKernels.DataCollection.Commands.Questionnaire;
using WB.Core.SharedKernels.DataCollection.Repositories;
using It = Machine.Specifications.It;

namespace WB.Core.BoundedContext.Capi.Synchronization.Tests.PullDataProcessorTests
{
    internal class when_sync_package_contains_information_about_delete_interview : PullDataProcessorTestContext
    {
        Establish context = () =>
        {
            interviewId = Guid.NewGuid();

            syncItem = new SyncItem() { ItemType = SyncItemType.DeleteQuestionnare, IsCompressed = false, Content = interviewId.ToString(), MetaInfo = "some metadata", Id = Guid.NewGuid() };

            commandService = new Mock<ICommandService>();

            plainQuestionnaireRepositoryMock = new Mock<IPlainQuestionnaireRepository>();

            changeLogManipulator = new Mock<IChangeLogManipulator>();

            cleanUpExecutorMock=new Mock<ICleanUpExecutor>();

            pullDataProcessor = CreatePullDataProcessor(changeLogManipulator.Object, commandService.Object, null, null,
                plainQuestionnaireRepositoryMock.Object, null, cleanUpExecutorMock.Object);
        };

        Because of = () => pullDataProcessor.Process(syncItem);

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

        It should_create_public_record_in_change_log_for_sync_item_once =
        () =>
            changeLogManipulator.Verify(
                x =>
                    x.CreatePublicRecord(syncItem.Id),
                Times.Once);

        private static PullDataProcessor pullDataProcessor;
        private static SyncItem syncItem;
        private static Mock<ICommandService> commandService;
        private static Mock<IPlainQuestionnaireRepository> plainQuestionnaireRepositoryMock;
        private static Mock<IChangeLogManipulator> changeLogManipulator;
        private static Mock<ICleanUpExecutor> cleanUpExecutorMock;
        private static Guid interviewId;
    }
}
