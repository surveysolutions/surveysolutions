﻿using System;
using Machine.Specifications;
using Moq;
using WB.Core.BoundedContexts.Capi.ChangeLog;
using WB.Core.BoundedContexts.Capi.Implementation.Services;
using WB.Core.BoundedContexts.Capi.Services;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.SharedKernel.Structures.Synchronization;
using WB.Core.SharedKernels.DataCollection.Repositories;
using It = Machine.Specifications.It;

namespace WB.Core.BoundedContexts.Capi.Tests.CapiDataSynchronizationServiceTests
{
    internal class when_sync_package_contains_information_about_delete_interview_and_delete_throw_exception : CapiDataSynchronizationServiceTestContext
    {
        Establish context = () =>
        {
            interviewId = Guid.NewGuid();

            syncItem = new SyncItem() { ItemType = SyncItemType.DeleteQuestionnare, IsCompressed = false, Content = interviewId.ToString(), MetaInfo = "some metadata", RootId = Guid.NewGuid() };

            commandService = new Mock<ICommandService>();

            plainQuestionnaireRepositoryMock = new Mock<IPlainQuestionnaireRepository>();

            changeLogManipulator = new Mock<IChangeLogManipulator>();

            cleanUpExecutorMock = new Mock<ICapiCleanUpService>();
            cleanUpExecutorMock.Setup(x => x.DeleteInterview(interviewId)).Throws<NullReferenceException>();

            capiDataSynchronizationService = CreateCapiDataSynchronizationService(changeLogManipulator.Object, commandService.Object, null, null,
                plainQuestionnaireRepositoryMock.Object, null, cleanUpExecutorMock.Object);
        };

        Because of = () => exception = Catch.Exception(() =>capiDataSynchronizationService.SavePulledItem(syncItem));

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
                    x => x.DeleteInterview(interviewId),
                    Times.Once);

        It should_not_create_public_record_in_change_log_for_sync_item =
        () =>
            changeLogManipulator.Verify(
                x =>
                    x.CreatePublicRecord(syncItem.RootId),
                Times.Never);

        It should_throw_NullReferenceException = () =>
            exception.ShouldBeOfType<NullReferenceException>();

        private static CapiDataSynchronizationService capiDataSynchronizationService;
        private static SyncItem syncItem;
        private static Mock<ICommandService> commandService;
        private static Mock<IPlainQuestionnaireRepository> plainQuestionnaireRepositoryMock;
        private static Mock<IChangeLogManipulator> changeLogManipulator;
        private static Mock<ICapiCleanUpService> cleanUpExecutorMock;
        private static Guid interviewId;
        private static Exception exception;
    }
}
