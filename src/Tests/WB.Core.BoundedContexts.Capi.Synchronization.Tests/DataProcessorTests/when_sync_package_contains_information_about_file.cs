using System;
using Machine.Specifications;
using Main.Core.Commands.File;
using Moq;
using Ncqrs.Commanding.ServiceModel;
using WB.Core.BoundedContexts.Capi.Synchronization.ChangeLog;
using WB.Core.BoundedContexts.Capi.Synchronization.Implementation;
using WB.Core.SharedKernel.Structures.Synchronization;
using WB.Core.SharedKernel.Utils.Serialization;
using It = Machine.Specifications.It;

namespace WB.Core.BoundedContext.Capi.Synchronization.Tests.DataProcessorTests
{
    internal class when_sync_package_contains_information_about_file : DataProcessorTestContext
    {
        Establish context = () =>
        {
            fileSyncDescription = new FileSyncDescription()
            {
                PublicKey = Guid.NewGuid(),
                Description = "desc",
                OriginalFile = "fo",
                Title = "t"
            };

            var jsonUtilsMock = new Mock<IJsonUtils>();
            jsonUtilsMock.Setup(x => x.Deserrialize<FileSyncDescription>(Moq.It.IsAny<string>())).Returns(fileSyncDescription);
            
            syncItem = new SyncItem() { ItemType = SyncItemType.File, IsCompressed = false, Content = "some content", Id = Guid.NewGuid() };

            commandService = new Mock<ICommandService>();

            changeLogManipulator = new Mock<IChangeLogManipulator>();
            dataProcessor = CreateDataProcessor(changeLogManipulator.Object, commandService.Object, jsonUtilsMock.Object);
        };

        Because of = () => dataProcessor.ProcessPulledItem(syncItem);

        It should_call_UploadFileCommand_once =
            () =>
                commandService.Verify(
                    x =>
                        x.Execute(
                            Moq.It.Is<UploadFileCommand>(
                                param =>
                                    param.Description == fileSyncDescription.Description &&
                                        param.OriginalFile == fileSyncDescription.OriginalFile &&
                                        param.PublicKey == fileSyncDescription.PublicKey && param.Title == fileSyncDescription.Title), null),
                    Times.Once);

        It should_create_public_record_in_change_log_for_sync_item_once =
        () =>
            changeLogManipulator.Verify(
                x =>
                    x.CreatePublicRecord(syncItem.Id),
                Times.Once);

        private static DataProcessor dataProcessor;
        private static SyncItem syncItem;
        private static FileSyncDescription fileSyncDescription;
        private static Mock<ICommandService> commandService;
        private static Mock<IChangeLogManipulator> changeLogManipulator;
    }
}
