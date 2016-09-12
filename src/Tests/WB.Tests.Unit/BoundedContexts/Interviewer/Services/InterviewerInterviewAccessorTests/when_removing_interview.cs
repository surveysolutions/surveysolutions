using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Machine.Specifications;
using Moq;
using Ncqrs.Eventing.Storage;
using Nito.AsyncEx.Synchronous;
using WB.Core.BoundedContexts.Interviewer.Implementation.Services;
using WB.Core.BoundedContexts.Interviewer.Implementation.Storage;
using WB.Core.BoundedContexts.Interviewer.Services.Infrastructure;
using WB.Core.BoundedContexts.Interviewer.Views;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.Infrastructure.WriteSide;
using WB.Core.SharedKernels.DataCollection.Commands.Interview;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure.Storage;
using WB.Tests.Unit.SharedKernels.SurveyManagement;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.BoundedContexts.Interviewer.Services.InterviewerInterviewAccessorTests
{
    internal class when_removing_interview
    {
        Establish context = () =>
        {
            var principal = Mock.Of<IInterviewerPrincipal>(x =>
                x.CurrentUserIdentity == Mock.Of<IInterviewerUserIdentity>(y => y.UserId == Guid.Parse("22222222222222222222222222222222")));

            eventStore = new Mock<IInterviewerEventStorage>();

            inMemoryMultimediaViewRepository = new SqliteInmemoryStorage<InterviewMultimediaView>();
            inMemoryMultimediaViewRepository.Store(interviewMultimediaViews);

            inMemoryFileViewRepository = new SqliteInmemoryStorage<InterviewFileView>();
            inMemoryFileViewRepository.Store(interviewFileViews);

            interviewerInterviewAccessor = Create.Service.InterviewerInterviewAccessor(
                commandService: mockOfCommandService.Object,
                interviewViewRepository: interviewViewRepositoryMock.Object,
                aggregateRootRepositoryWithCache: mockOfAggregateRootRepositoryWithCache.Object,
                snapshotStoreWithCache: mockOfSnapshotStoreWithCache.Object,
                principal: principal,
                eventStore: eventStore.Object,
                interviewMultimediaViewRepository: inMemoryMultimediaViewRepository,
                interviewFileViewRepository: inMemoryFileViewRepository);
        };

        Because of = () => interviewerInterviewAccessor.RemoveInterview(interviewId);

        It should_remove_interview_view_from_plain_storage = () =>
            interviewViewRepositoryMock.Verify(x => x.Remove(interviewStringId), Times.Once);

        It should_clean_cache_of_aggregate_root_repository = () =>
            mockOfAggregateRootRepositoryWithCache.Verify(x => x.CleanCache(), Times.Once);

        It should_clean_cache_of_snapshot_repository = () =>
            mockOfSnapshotStoreWithCache.Verify(x => x.CleanCache(), Times.Once);

        It should_not_multimedia_repository_contains_views_by_specified_id = () =>
            inMemoryMultimediaViewRepository.Where(multimedia => multimedia.InterviewId == interviewId).Any().ShouldBeFalse();

        It should_not_file_repository_contains_views_by_specified_interview_id = () =>
            inMemoryFileViewRepository.Where(file => file.Id == interviewFile1 || file.Id == interviewFile2).Any().ShouldBeFalse();

        It should_remove_events_by_specified_interview = () =>
             eventStore.Verify(x => x.RemoveEventSourceById(interviewId), Times.Once);


        private static readonly string interviewStringId = "11111111111111111111111111111111";
        private static readonly Guid interviewId = Guid.Parse(interviewStringId);
        private static readonly InterviewMultimediaView[] interviewMultimediaViews =
        {
            new InterviewMultimediaView
            {
                Id = Guid.NewGuid().FormatGuid(),
                InterviewId = interviewId, FileId = interviewFile1
            },
            new InterviewMultimediaView
            {
                Id = Guid.NewGuid().FormatGuid(),
                InterviewId = interviewId, FileId = interviewFile2
            },
            new InterviewMultimediaView
            {
                Id = Guid.NewGuid().FormatGuid(),
                InterviewId = Guid.Parse("44444444444444444444444444444444")
            },
        };
        private static readonly InterviewFileView[] interviewFileViews =
        {
            new InterviewFileView { Id = interviewFile1},
            new InterviewFileView { Id = interviewFile2},
            new InterviewFileView { Id = "file by another interview"},
        };

        const string interviewFile1 = "file 1";
        const string interviewFile2 = "file 2";

        private static readonly Mock<ICommandService> mockOfCommandService = new Mock<ICommandService>();
        private static readonly Mock<IAsyncPlainStorage<InterviewView>> interviewViewRepositoryMock = new Mock<IAsyncPlainStorage<InterviewView>>();
        private static readonly Mock<IEventSourcedAggregateRootRepositoryWithCache> mockOfAggregateRootRepositoryWithCache = new Mock<IEventSourcedAggregateRootRepositoryWithCache>();
        private static readonly Mock<ISnapshotStoreWithCache> mockOfSnapshotStoreWithCache = new Mock<ISnapshotStoreWithCache>();
        private static InterviewerInterviewAccessor interviewerInterviewAccessor;
        private static Mock<IInterviewerEventStorage> eventStore;
        private static IAsyncPlainStorage<InterviewMultimediaView> inMemoryMultimediaViewRepository;
        private static IAsyncPlainStorage<InterviewFileView> inMemoryFileViewRepository;
    }
}
