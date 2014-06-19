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
using WB.Core.SharedKernel.Utils.Compression;
using WB.Core.SharedKernel.Utils.Serialization;
using WB.Core.SharedKernels.DataCollection.Commands.Interview;
using WB.Core.SharedKernels.DataCollection.DataTransferObjects.Synchronization;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;
using It = Machine.Specifications.It;

namespace WB.Core.BoundedContext.Capi.Synchronization.Tests.SyncPackageRestoreServiceTests
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
            jsonUtilsMock.Setup(x => x.Deserrialize<InterviewSynchronizationDto>("some string")).Returns(interviewSynchronizationDto);

            commandServiceMock=new Mock<ICommandService>();

            syncPackageRestoreService = CreateSyncPackageRestoreService(capiSynchronizationCacheServiceMock.Object, jsonUtilsMock.Object, commandServiceMock.Object);
        };

        Because of = () => result = syncPackageRestoreService.CheckAndApplySyncPackage(interviewSynchronizationDto.Id);

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

        It should_result_be_true = () => result.ShouldBeTrue();

        It should_synchronization_item_be_deleted = () => capiSynchronizationCacheServiceMock.Verify(x => x.DeleteItem(interviewSynchronizationDto.Id), Times.Once);

        private static SyncPackageRestoreService syncPackageRestoreService;
        private static bool result;
        private static InterviewSynchronizationDto interviewSynchronizationDto;
        private static Mock<ICapiSynchronizationCacheService> capiSynchronizationCacheServiceMock;
        private static Mock<IJsonUtils> jsonUtilsMock;
        private static Mock<ICommandService> commandServiceMock;
    }
}
