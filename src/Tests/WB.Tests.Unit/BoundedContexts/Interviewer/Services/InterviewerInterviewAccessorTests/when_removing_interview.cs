using System;
using System.Linq;
using FluentAssertions;
using Moq;
using Ncqrs.Eventing.Storage;
using NUnit.Framework;
using WB.Core.BoundedContexts.Interviewer.Implementation.Services;
using WB.Core.BoundedContexts.Interviewer.Services.Infrastructure;
using WB.Core.BoundedContexts.Interviewer.Views;
using WB.Core.BoundedContexts.Interviewer.Views.Dashboard;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.Infrastructure.WriteSide;
using WB.Core.SharedKernels.Enumerator.Implementation.Services;
using WB.Core.SharedKernels.Enumerator.Services;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure.Storage;
using WB.Core.SharedKernels.Enumerator.Views;
using WB.Tests.Abc;
using WB.Tests.Abc.Storage;

namespace WB.Tests.Unit.BoundedContexts.Interviewer.Services.InterviewerInterviewAccessorTests
{
    internal class when_removing_interview
    {
        [OneTimeSetUp]
        public void context()
        {
            var principal = Mock.Of<IInterviewerPrincipal>(x =>
                x.CurrentUserIdentity == Mock.Of<IInterviewerUserIdentity>(y => y.UserId == Guid.Parse("22222222222222222222222222222222")));

            eventStore = new Mock<IEnumeratorEventStorage>();

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

            BecauseOf();
        }

        public void BecauseOf() => interviewerInterviewAccessor.RemoveInterview(interviewId);

        [Test]
        public void should_remove_interview_view_from_plain_storage() =>
            interviewViewRepositoryMock.Verify(x => x.Remove(interviewStringId), Times.Once);


        [Test]
        public void should_clean_cache_of_aggregate_root_repository() =>
            mockOfAggregateRootRepositoryWithCache.Verify(x => x.CleanCache(), Times.Once);


        [Test]
        public void should_not_multimedia_repository_contains_views_by_specified_id() =>
            inMemoryMultimediaViewRepository.Where(multimedia => multimedia.InterviewId == interviewId).Any().Should().BeFalse();

        [Test]
        public void should_not_file_repository_contains_views_by_specified_interview_id() =>
            inMemoryFileViewRepository.Where(file => file.Id == interviewFile1 || file.Id == interviewFile2).Any().Should().BeFalse();

        [Test]
        public void should_remove_events_by_specified_interview() =>
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
        private static readonly Mock<IPlainStorage<InterviewView>> interviewViewRepositoryMock = new Mock<IPlainStorage<InterviewView>>();
        private static readonly Mock<IEventSourcedAggregateRootRepositoryWithCache> mockOfAggregateRootRepositoryWithCache = new Mock<IEventSourcedAggregateRootRepositoryWithCache>();
        private static readonly Mock<ISnapshotStoreWithCache> mockOfSnapshotStoreWithCache = new Mock<ISnapshotStoreWithCache>();
        private static InterviewerInterviewAccessor interviewerInterviewAccessor;
        private static Mock<IEnumeratorEventStorage> eventStore;
        private static IPlainStorage<InterviewMultimediaView> inMemoryMultimediaViewRepository;
        private static IPlainStorage<InterviewFileView> inMemoryFileViewRepository;
    }
}
