using Moq;
using Ncqrs.Eventing.Storage;
using WB.Core.BoundedContexts.Interviewer.Implementation.Services;
using WB.Core.BoundedContexts.Interviewer.Services.Infrastructure;
using WB.Core.BoundedContexts.Interviewer.Views;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.Infrastructure.WriteSide;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure.Storage;

namespace WB.Tests.Unit.BoundedContexts.Interviewer.Services.InterviewerInterviewAccessorTests
{
    internal class InterviewerInterviewAccessorTestsContext
    {
        public static InterviewerInterviewAccessor CreateInterviewerInterviewAccessor(
            IAsyncPlainStorage<QuestionnaireView> questionnaireRepository = null,
            IAsyncPlainStorage<EventView> eventRepository = null,
            IAsyncPlainStorage<InterviewView> interviewViewRepository = null,
            IAsyncPlainStorage<InterviewMultimediaView> interviewMultimediaViewRepository = null,
            IAsyncPlainStorage<InterviewFileView> interviewFileViewRepository = null,
            ICommandService commandService = null,
            IInterviewerPrincipal principal = null,
            ISerializer serializer = null,
            IStringCompressor compressor = null,
            IEventStore eventStore = null,
            IAggregateRootRepositoryWithCache aggregateRootRepositoryWithCache = null,
            ISnapshotStoreWithCache snapshotStoreWithCache = null)
        {
            return new InterviewerInterviewAccessor(
                questionnaireRepository: questionnaireRepository ?? Mock.Of<IAsyncPlainStorage<QuestionnaireView>>(),
                eventRepository: eventRepository ?? Mock.Of<IAsyncPlainStorage<EventView>>(),
                interviewViewRepository: interviewViewRepository ?? Mock.Of<IAsyncPlainStorage<InterviewView>>(),
                interviewMultimediaViewRepository: interviewMultimediaViewRepository ?? Mock.Of<IAsyncPlainStorage<InterviewMultimediaView>>(),
                interviewFileViewRepository: interviewFileViewRepository ?? Mock.Of<IAsyncPlainStorage<InterviewFileView>>(),
                commandService: commandService ?? Mock.Of<ICommandService>(),
                principal: principal ?? Mock.Of<IInterviewerPrincipal>(),
                serializer: serializer ?? Mock.Of<ISerializer>(),
                compressor: compressor ?? Mock.Of<IStringCompressor>(),
                eventStore: eventStore ?? Mock.Of<IEventStore>(),
                aggregateRootRepositoryWithCache: aggregateRootRepositoryWithCache ?? Mock.Of<IAggregateRootRepositoryWithCache>(),
                snapshotStoreWithCache: snapshotStoreWithCache ?? Mock.Of<ISnapshotStoreWithCache>());
        }
    }
}
