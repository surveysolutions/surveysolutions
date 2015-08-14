using System;
using System.Collections.Generic;
using Machine.Specifications;
using Moq;
using WB.Core.BoundedContexts.Capi.Implementation.Services;
using WB.Core.BoundedContexts.Capi.Services;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.SharedKernels.DataCollection.Commands.Interview;
using WB.Core.SharedKernels.DataCollection.DataTransferObjects.Synchronization;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;
using WB.Core.SharedKernels.SurveySolutions.Services;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.BoundedContexts.Capi.SyncPackageRestoreServiceTests
{
    internal class when_synchronization_item_exists : SyncPackageRestoreServiceTestContext
    {
        Establish context = () =>
        {
            interviewSynchronizationDto = new InterviewSynchronizationDto(Guid.NewGuid(), InterviewStatus.InterviewerAssigned, null,
                Guid.NewGuid(), Guid.NewGuid(), 1, new AnsweredQuestionSynchronizationDto[0], new HashSet<InterviewItemId>(),
                new HashSet<InterviewItemId>(), new HashSet<InterviewItemId>(), new HashSet<InterviewItemId>(),
                new Dictionary<InterviewItemId, int>(), new Dictionary<InterviewItemId, RosterSynchronizationDto[]>(), false, false);

            capiSynchronizationCacheServiceMock = new Mock<ICapiSynchronizationCacheService>();
            capiSynchronizationCacheServiceMock.Setup(x => x.DoesCachedItemExist(Moq.It.IsAny<Guid>())).Returns(true);
            capiSynchronizationCacheServiceMock.Setup(x => x.LoadItem(Moq.It.IsAny<Guid>())).Returns("some string");

            jsonUtilsMock=new Mock<IJsonUtils>();
            jsonUtilsMock.Setup(x => x.Deserialize<InterviewSynchronizationDto>("some string")).Returns(interviewSynchronizationDto);

            commandServiceMock=new Mock<ICommandService>();

            syncPackageRestoreService = CreateSyncPackageRestoreService(capiSynchronizationCacheServiceMock.Object, jsonUtilsMock.Object, commandServiceMock.Object);
        };

        Because of = () => syncPackageRestoreService.CheckAndApplySyncPackage(interviewSynchronizationDto.Id);

        It should_SynchronizeInterviewCommand_be_called =
            () =>
                commandServiceMock.Verify(
                    x =>
                        x.Execute(
                            Moq.It.Is<SynchronizeInterviewCommand>(
                                param =>
                                    param.InterviewId == interviewSynchronizationDto.Id &&
                                        param.SynchronizedInterview == interviewSynchronizationDto &&
                                        param.UserId == interviewSynchronizationDto.UserId), null), Times.Once);

        It should_synchronization_item_be_deleted = () => capiSynchronizationCacheServiceMock.Verify(x => x.DeleteItem(interviewSynchronizationDto.Id), Times.Once);

        private static SyncPackageRestoreService syncPackageRestoreService;
        private static InterviewSynchronizationDto interviewSynchronizationDto;
        private static Mock<ICapiSynchronizationCacheService> capiSynchronizationCacheServiceMock;
        private static Mock<IJsonUtils> jsonUtilsMock;
        private static Mock<ICommandService> commandServiceMock;
    }
}
