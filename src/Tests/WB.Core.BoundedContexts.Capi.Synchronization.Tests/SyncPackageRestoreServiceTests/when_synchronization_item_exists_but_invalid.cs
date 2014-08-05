using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Machine.Specifications;
using Moq;
using Ncqrs.Commanding.ServiceModel;
using WB.Core.BoundedContexts.Capi.Synchronization.Implementation.Services;
using WB.Core.BoundedContexts.Capi.Synchronization.Services;
using WB.Core.SharedKernel.Utils.Serialization;
using WB.Core.SharedKernels.DataCollection.Commands.Interview;
using WB.Core.SharedKernels.DataCollection.DataTransferObjects.Synchronization;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;
using It = Machine.Specifications.It;

namespace WB.Core.BoundedContext.Capi.Synchronization.Tests.SyncPackageRestoreServiceTests
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
            jsonUtilsMock.Setup(x => x.Deserrialize<InterviewSynchronizationDto>("some string")).Returns(interviewSynchronizationDto);

            commandServiceMock = new Mock<ICommandService>();
            commandServiceMock.Setup(x => x.Execute(Moq.It.IsAny<SynchronizeInterviewCommand>(), null)).Throws<NullReferenceException>();
            syncPackageRestoreService = CreateSyncPackageRestoreService(capiSynchronizationCacheServiceMock.Object, jsonUtilsMock.Object, commandServiceMock.Object);
        };

        Because of = () => result = syncPackageRestoreService.CheckAndApplySyncPackage(interviewSynchronizationDto.Id);

        It should_result_be_false = () => result.ShouldBeFalse();

        It should_synchronization_item_be_not_deleted = () => capiSynchronizationCacheServiceMock.Verify(x => x.DeleteItem(interviewSynchronizationDto.Id), Times.Never);

        private static SyncPackageRestoreService syncPackageRestoreService;
        private static bool result;
        private static InterviewSynchronizationDto interviewSynchronizationDto;
        private static Mock<ICapiSynchronizationCacheService> capiSynchronizationCacheServiceMock;
        private static Mock<IJsonUtils> jsonUtilsMock;
        private static Mock<ICommandService> commandServiceMock;
    }
}
