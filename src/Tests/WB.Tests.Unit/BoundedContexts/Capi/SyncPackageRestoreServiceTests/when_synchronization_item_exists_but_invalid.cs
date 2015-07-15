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
    internal class when_synchronization_item_exists_but_invalid : SyncPackageRestoreServiceTestContext
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

            jsonUtilsMock = new Mock<IJsonUtils>();
            jsonUtilsMock.Setup(x => x.Deserialize<InterviewSynchronizationDto>("some string")).Returns(interviewSynchronizationDto);

            commandServiceMock = new Mock<ICommandService>();
            commandServiceMock.Setup(x => x.Execute(Moq.It.IsAny<SynchronizeInterviewCommand>(), null, false)).Throws<NullReferenceException>();
            syncPackageRestoreService = CreateSyncPackageRestoreService(capiSynchronizationCacheServiceMock.Object, jsonUtilsMock.Object, commandServiceMock.Object);
        };
        Because of = () =>
            exception = Catch.Exception(() =>
                syncPackageRestoreService.CheckAndApplySyncPackage(interviewSynchronizationDto.Id));

        It should_result_be_false = () => exception.ShouldNotBeNull();

        It should_synchronization_item_be_not_deleted = () => capiSynchronizationCacheServiceMock.Verify(x => x.DeleteItem(interviewSynchronizationDto.Id), Times.Never);

        private static SyncPackageRestoreService syncPackageRestoreService;
        private static InterviewSynchronizationDto interviewSynchronizationDto;
        private static Mock<ICapiSynchronizationCacheService> capiSynchronizationCacheServiceMock;
        private static Mock<IJsonUtils> jsonUtilsMock;
        private static Mock<ICommandService> commandServiceMock;
        private static Exception exception;
    }
}
