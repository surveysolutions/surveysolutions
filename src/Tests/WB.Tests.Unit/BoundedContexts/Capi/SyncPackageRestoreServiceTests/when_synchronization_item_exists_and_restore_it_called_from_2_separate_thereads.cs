using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Machine.Specifications;
using Moq;
using WB.Core.BoundedContexts.Capi.Implementation.Services;
using WB.Core.BoundedContexts.Capi.Services;
using WB.Core.GenericSubdomains.Utils.Services;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.SharedKernels.DataCollection.Commands.Interview;
using WB.Core.SharedKernels.DataCollection.DataTransferObjects.Synchronization;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.BoundedContexts.Capi.SyncPackageRestoreServiceTests
{
    internal class when_synchronization_item_exists_and_restore_it_called_from_2_separate_thereads : SyncPackageRestoreServiceTestContext
    {
        Establish context = () =>
        {
            interviewSynchronizationDto = new InterviewSynchronizationDto(Guid.NewGuid(), InterviewStatus.InterviewerAssigned, null,
                Guid.NewGuid(), Guid.NewGuid(), 1, new AnsweredQuestionSynchronizationDto[0], new HashSet<InterviewItemId>(),
                new HashSet<InterviewItemId>(), new HashSet<InterviewItemId>(), new HashSet<InterviewItemId>(),
                new Dictionary<InterviewItemId, int>(), new Dictionary<InterviewItemId, RosterSynchronizationDto[]>(), false, false);

            capiSynchronizationCacheServiceMock = new CapiSynchronizationCacheServiceMock();

            jsonUtilsMock = new Mock<IJsonUtils>();
            jsonUtilsMock.Setup(x => x.Deserialize<InterviewSynchronizationDto>("some string")).Returns(interviewSynchronizationDto);

            commandServiceMock = new Mock<ICommandService>();

            syncPackageRestoreService = CreateSyncPackageRestoreService(capiSynchronizationCacheServiceMock, jsonUtilsMock.Object, commandServiceMock.Object);
        };

        Because of = () =>
        {
            List<Task> tasks = new List<Task>();
            for (int i = 0; i < 30; i++)
            {
                tasks.Add(Task.Run(() => syncPackageRestoreService.CheckAndApplySyncPackage(interviewSynchronizationDto.Id)));
            }

            Task.WaitAll(tasks.ToArray());
        };

        It should_call_SynchronizeInterviewCommand =
            () =>
                commandServiceMock.Verify(
                    x =>
                        x.Execute(
                            Moq.It.Is<SynchronizeInterviewCommand>(
                                param =>
                                    param.InterviewId == interviewSynchronizationDto.Id &&
                                        param.SynchronizedInterview == interviewSynchronizationDto &&
                                        param.UserId == interviewSynchronizationDto.UserId), null), Times.Once);

        It should_delete_synchronization_item = () => capiSynchronizationCacheServiceMock.DeleteCallCount.ShouldEqual(1);

        static SyncPackageRestoreService syncPackageRestoreService;
        static InterviewSynchronizationDto interviewSynchronizationDto;
        static CapiSynchronizationCacheServiceMock capiSynchronizationCacheServiceMock;
        static Mock<IJsonUtils> jsonUtilsMock;
        static Mock<ICommandService> commandServiceMock;
    }

    class CapiSynchronizationCacheServiceMock : ICapiSynchronizationCacheService
    {
        public int DeleteCallCount { get; private set; }
        public bool SaveItem(Guid itemId, string itemContent)
        {
            throw new NotImplementedException();
        }

        public string LoadItem(Guid itemId)
        {
            return "some string";
        }

        public bool DoesCachedItemExist(Guid itemId)
        {
            return this.DeleteCallCount == 0;
        }

        public bool DeleteItem(Guid itemId)
        {
            this.DeleteCallCount++;
            return true;
        }
    }
}
