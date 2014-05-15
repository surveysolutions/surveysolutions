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
using WB.Core.GenericSubdomains.Logging;
using WB.Core.GenericSubdomains.Utils;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernel.Structures.Synchronization;
using WB.Core.SharedKernel.Utils.Serialization;
using WB.Core.SharedKernels.SurveyManagement.Views.Interview;
using It = Machine.Specifications.It;
using it = Moq.It;

namespace WB.Core.BoundedContexts.Supervisor.Tests.Synchronization.InterviewsSynchronizerTests
{
    internal class when_pushing_interviews_and_one_interview_is_ready_and_has_one_event_with_empty_origin : InterviewsSynchronizerTestsContext
    {
        Establish context = () =>
        {
            string positiveResponse = ":)";

            loggerMock
                .Setup(logger => logger.Error(it.IsAny<string>(), it.IsAny<Exception>()))
                .Callback<string, Exception>((error, exception) => lastLoggedException = exception);

            var httpMessageHandler = Mock.Of<HttpMessageHandler>();

            Mock.Get(httpMessageHandler)
                .Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
                .Returns(Task<HttpResponseMessage>.Factory.StartNew(() =>
                    new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent(positiveResponse) }))
                .Callback<HttpRequestMessage, CancellationToken>((message, token) =>
                    contentSentToHq = message.Content.ReadAsStringAsync().Result);

            var readyToSendInterviewsRepositoryWriter = Mock.Of<IQueryableReadSideRepositoryWriter<ReadyToSendToHeadquartersInterview>>(writer
                => writer.QueryAll(it.IsAny<Expression<Func<ReadyToSendToHeadquartersInterview, bool>>>()) == new [] { new ReadyToSendToHeadquartersInterview(interviewId) });

            interviewEvent = Create.CommittedEvent(eventSourceId: interviewId, origin: null);
            var eventStore = Mock.Of<IEventStore>(store
                => store.ReadFrom(interviewId, 0, it.IsAny<long>()) == new CommittedEventStream(interviewId, interviewEvent));

            var interviewSummaryRepositoryWriter = Mock.Of<IReadSideRepositoryWriter<InterviewSummary>>(writer
                => writer.GetById(interviewId.FormatGuid()) == Create.InterviewSummary());

            var jsonUtils = Mock.Of<IJsonUtils>(utils
                => utils.Deserrialize<bool>(positiveResponse) == true);

            Mock.Get(jsonUtils)
                .Setup(utils => utils.GetItemAsContent(it.IsAny<AggregateRootEvent[]>()))
                .Returns(eventsJson)
                .Callback<object>(entity => events = (AggregateRootEvent[]) entity);

            Mock.Get(jsonUtils)
                .Setup(utils => utils.GetItemAsContent(it.IsAny<InterviewMetaInfo>()))
                .Returns(metadataJson)
                .Callback<object>(entity => metadata = (InterviewMetaInfo) entity);

            Mock.Get(jsonUtils)
                .Setup(utils => utils.GetItemAsContent(it.IsAny<SyncItem>()))
                .Returns(syncItemJson)
                .Callback<object>(entity => syncItem = (SyncItem) entity);

            interviewsSynchronizer = Create.InterviewsSynchronizer(
                readyToSendInterviewsRepositoryWriter: readyToSendInterviewsRepositoryWriter,
                interviewSummaryRepositoryWriter: interviewSummaryRepositoryWriter,
                eventStore: eventStore,
                logger: loggerMock.Object,
                jsonUtils: jsonUtils,
                httpMessageHandler: httpMessageHandler);
        };

        Because of = () =>
            interviewsSynchronizer.Push(userId);

        It should_not_log_errors = () =>
            loggerMock.Verify(logger => logger.Error(it.IsAny<string>(), it.IsAny<Exception>()), Times.Never);

        It should_not_log_exceptions = () =>
            lastLoggedException.ShouldBeNull();

        It should_send_sync_item_json_to_hq = () =>
            contentSentToHq.ShouldEqual(syncItemJson);

        It should_set_sync_item_content_to_compressed_events_json = () =>
            PackageHelper.DecompressString(syncItem.Content).ShouldEqual(eventsJson);

        It should_set_sync_item_meta_info_to_metadata_json = () =>
            PackageHelper.DecompressString(syncItem.MetaInfo).ShouldEqual(metadataJson);

        It should_set_sync_item_compression_flag_to_true = () =>
            syncItem.IsCompressed.ShouldBeTrue();

        It should_set_sync_item_id_to_interview_id = () =>
            syncItem.Id.ShouldEqual(interviewId);

        It should_set_sync_item_type_to_questionnaire = () =>
            syncItem.ItemType.ShouldEqual("q");

        It should_set_metadata_public_key_to_interview_id = () =>
            metadata.PublicKey.ShouldEqual(interviewId);

        It should_put_interview_event_to_events = () =>
            events.Single().Payload.ShouldEqual(interviewEvent.Payload);

        private static InterviewsSynchronizer interviewsSynchronizer;
        private static Guid userId = Guid.Parse("DDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDD");
        private static Guid interviewId = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
        private static Mock<ILogger> loggerMock = new Mock<ILogger>();
        private static Exception lastLoggedException;
        private static string contentSentToHq;
        private static string syncItemJson = "sync item json";
        private static string eventsJson = "events json";
        private static string metadataJson = "metadata json";
        private static AggregateRootEvent[] events;
        private static InterviewMetaInfo metadata;
        private static SyncItem syncItem;
        private static CommittedEvent interviewEvent;
    }
}