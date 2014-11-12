﻿using System;
using Machine.Specifications;
using Main.Core.Commands.File;
using Moq;
using WB.Core.BoundedContexts.Capi.ChangeLog;
using WB.Core.BoundedContexts.Capi.Implementation.Services;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.SharedKernel.Structures.Synchronization;
using WB.Core.SharedKernel.Utils.Serialization;
using It = Machine.Specifications.It;

namespace WB.Core.BoundedContexts.Capi.Tests.CapiDataSynchronizationServiceTests
{
    internal class when_sync_package_contains_information_about_file : CapiDataSynchronizationServiceTestContext
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
            capiDataSynchronizationService = CreateCapiDataSynchronizationService(changeLogManipulator.Object, commandService.Object, jsonUtilsMock.Object);
        };

        Because of = () => capiDataSynchronizationService.SavePulledItem(syncItem);

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

        private static CapiDataSynchronizationService capiDataSynchronizationService;
        private static SyncItem syncItem;
        private static FileSyncDescription fileSyncDescription;
        private static Mock<ICommandService> commandService;
        private static Mock<IChangeLogManipulator> changeLogManipulator;
    }
}
