using System;
using System.Linq;
using System.Linq.Expressions;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Machine.Specifications;
using Main.Core;
using Main.Core.Events;
using Moq;
using Moq.Protected;
using Ncqrs.Eventing;
using Ncqrs.Eventing.Storage;
using WB.Core.BoundedContexts.Supervisor.Interviews.Implementation.Views;
using WB.Core.BoundedContexts.Supervisor.Synchronization.Implementation;
using WB.Core.GenericSubdomains.Utils;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernel.Structures.Synchronization;
using WB.Core.SharedKernel.Utils.Serialization;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.SurveyManagement.Views.Interview;
using It = Machine.Specifications.It;
using it = Moq.It;

namespace WB.Core.BoundedContexts.Supervisor.Tests.Synchronization.InterviewsSynchronizerTests
{
    internal class when_pushing_interviews_and_one_interview_is_ready_and_has_different_events : InterviewsSynchronizerTestsContext
    {
        Establish context = () =>
        {
            string positiveResponse = ":)";

            var eventStream = new CommittedEventStream(interviewId, new []
            {
                Create.CommittedEvent(origin: null, eventIdentifier: Guid.Parse("11111111111111111111111111111111"), eventSourceId: interviewId, eventSequence: 1),
                Create.CommittedEvent(origin: null, eventIdentifier: Guid.Parse("22222222222222222222222222222222"), eventSourceId: interviewId, eventSequence: 2),
                Create.CommittedEvent(origin: "hq-sync", eventIdentifier: Guid.Parse("33333333333333333333333333333333"), payload: new InterviewSentToHeadquarters(), eventSourceId: interviewId, eventSequence: 3),
                Create.CommittedEvent(origin: null, eventIdentifier: Guid.Parse("44444444444444444444444444444444"), eventSourceId: interviewId, eventSequence: 4),
                Create.CommittedEvent(origin: "hq-sync", eventIdentifier: Guid.Parse("55555555555555555555555555555555"), eventSourceId: interviewId, eventSequence: 5),
                Create.CommittedEvent(origin: null, eventIdentifier: Guid.Parse("66666666666666666666666666666666"), payload: new InterviewSentToHeadquarters(), eventSourceId: interviewId, eventSequence: 6),
                Create.CommittedEvent(origin: null, eventIdentifier: Guid.Parse("77777777777777777777777777777777"), eventSourceId: interviewId, eventSequence: 7),
                Create.CommittedEvent(origin: "hq-sync", eventIdentifier: Guid.Parse("88888888888888888888888888888888"), eventSourceId: interviewId, eventSequence: 8),
                Create.CommittedEvent(origin: null, eventIdentifier: Guid.Parse("99999999999999999999999999999999"), eventSourceId: interviewId, eventSequence: 9),
                Create.CommittedEvent(origin: "capi-sync", eventIdentifier: Guid.Parse("19999999999999999999999999999999"), eventSourceId: interviewId, eventSequence: 10)
            });

            eventsBeforeLastPush = new[]
            {
                Guid.Parse("11111111111111111111111111111111"),
                Guid.Parse("22222222222222222222222222222222"),
                Guid.Parse("33333333333333333333333333333333"),
                Guid.Parse("44444444444444444444444444444444"),
                Guid.Parse("55555555555555555555555555555555"),
            };

            pushEvents = new[]
            {
                Guid.Parse("33333333333333333333333333333333"),
                Guid.Parse("66666666666666666666666666666666"),
            };

            hqSynchronizationEvents = new[]
            {
                Guid.Parse("33333333333333333333333333333333"),
                Guid.Parse("55555555555555555555555555555555"),
                Guid.Parse("88888888888888888888888888888888"),
            };

            capiSynchronizationEvents = new[]
            {
                Guid.Parse("19999999999999999999999999999999")
            };

            eventsWithEmptyOriginAfterLastPush = new[]
            {
                Guid.Parse("77777777777777777777777777777777"),
                Guid.Parse("99999999999999999999999999999999"),
            };

            var httpMessageHandler = Mock.Of<HttpMessageHandler>();

            Mock.Get(httpMessageHandler)
                .Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
                .Returns(Task<HttpResponseMessage>.Factory.StartNew(() =>
                    new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent(positiveResponse) }));

            var readyToSendInterviewsRepositoryWriter = Mock.Of<IQueryableReadSideRepositoryWriter<ReadyToSendToHeadquartersInterview>>(writer
                => writer.QueryAll(it.IsAny<Expression<Func<ReadyToSendToHeadquartersInterview, bool>>>()) == new[] { new ReadyToSendToHeadquartersInterview(interviewId) });

            var eventStore = Mock.Of<IEventStore>(store
                => store.ReadFrom(interviewId, 0, it.IsAny<long>()) == eventStream);

            var interviewSummaryRepositoryWriter = Mock.Of<IReadSideRepositoryWriter<InterviewSummary>>(writer
                => writer.GetById(interviewId.FormatGuid()) == Create.InterviewSummary());

            var jsonUtils = Mock.Of<IJsonUtils>(utils
                => utils.GetItemAsContent(it.IsAny<InterviewMetaInfo>()) == "metadata json"
                && utils.GetItemAsContent(it.IsAny<SyncItem>()) == "sync item json"
                && utils.Deserrialize<bool>(positiveResponse) == true);

            Mock.Get(jsonUtils)
                .Setup(utils => utils.GetItemAsContent(it.IsAny<AggregateRootEvent[]>()))
                .Returns("events json")
                .Callback<object>(entity => events = (AggregateRootEvent[]) entity);

            interviewsSynchronizer = Create.InterviewsSynchronizer(
                readyToSendInterviewsRepositoryWriter: readyToSendInterviewsRepositoryWriter,
                interviewSummaryRepositoryWriter: interviewSummaryRepositoryWriter,
                eventStore: eventStore,
                jsonUtils: jsonUtils,
                httpMessageHandler: () => httpMessageHandler);
        };

        Because of = () =>
            interviewsSynchronizer.Push(userId);

        It should_not_push_events_before_last_push = () =>
            events.Select(e => e.EventIdentifier).ShouldNotContain(eventsBeforeLastPush);

        It should_not_push_events_which_mean_that_interview_is_pushed = () =>
            events.Select(e => e.EventIdentifier).ShouldNotContain(pushEvents);

        It should_not_push_events_produced_by_synchronization_with_hq = () =>
            events.Select(e => e.EventIdentifier).ShouldNotContain(hqSynchronizationEvents);

        It should_not_push_events_produced_by_synchronization_meta = () =>
            events.Select(e => e.EventIdentifier).ShouldNotContain(capiSynchronizationEvents);

        It should_push_only_events_with_empty_origin_after_last_push = () =>
            events.Select(e => e.EventIdentifier).ShouldContainOnly(eventsWithEmptyOriginAfterLastPush);

        private static InterviewsSynchronizer interviewsSynchronizer;
        private static Guid userId = Guid.Parse("DDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDD");
        private static Guid interviewId = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
        private static AggregateRootEvent[] events;
        private static Guid[] eventsBeforeLastPush;
        private static Guid[] pushEvents;
        private static Guid[] hqSynchronizationEvents;
        private static Guid[] eventsWithEmptyOriginAfterLastPush;
        private static Guid[] capiSynchronizationEvents;
    }
}