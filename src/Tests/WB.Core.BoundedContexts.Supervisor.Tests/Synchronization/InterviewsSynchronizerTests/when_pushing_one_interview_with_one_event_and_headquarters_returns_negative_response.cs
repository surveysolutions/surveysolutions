using System;
using System.Linq.Expressions;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Machine.Specifications;
using Main.Core.Events;
using Moq;
using Moq.Protected;
using Ncqrs.Eventing;
using Ncqrs.Eventing.Storage;
using WB.Core.BoundedContexts.Supervisor.Interviews.Implementation.Views;
using WB.Core.BoundedContexts.Supervisor.Synchronization;
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
    internal class when_pushing_one_interview_with_one_event_and_headquarters_returns_negative_response : InterviewsSynchronizerTestsContext
    {
        Establish context = () =>
        {
            string negativeResponse = ":(";
            Guid interviewId = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");

            var httpMessageHandler = Mock.Of<HttpMessageHandler>();

            Mock.Get(httpMessageHandler)
                .Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
                .Returns(Task<HttpResponseMessage>.Factory.StartNew(() =>
                    new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent(negativeResponse) }));

            var readyToSendInterviewsRepositoryWriter = Mock.Of<IQueryableReadSideRepositoryWriter<ReadyToSendToHeadquartersInterview>>(writer
                => writer.QueryAll(it.IsAny<Expression<Func<ReadyToSendToHeadquartersInterview, bool>>>()) == new[] { new ReadyToSendToHeadquartersInterview(interviewId) });

            CommittedEvent interviewEvent = Create.CommittedEvent(eventSourceId: interviewId, origin: null);
            var eventStore = Mock.Of<IEventStore>(store
                => store.ReadFrom(interviewId, 0, it.IsAny<long>()) == new CommittedEventStream(interviewId, interviewEvent));

            var interviewSummaryRepositoryWriter = Mock.Of<IReadSideRepositoryWriter<InterviewSummary>>(writer
                => writer.GetById(interviewId.FormatGuid()) == Create.InterviewSummary());

            var jsonUtils = Mock.Of<IJsonUtils>(utils
                => utils.GetItemAsContent(it.IsAny<InterviewMetaInfo>()) == "metadata json"
                && utils.GetItemAsContent(it.IsAny<AggregateRootEvent[]>()) == "events json"
                && utils.GetItemAsContent(it.IsAny<SyncItem>()) == "sync item json"
                && utils.Deserrialize<bool>(negativeResponse) == false);

            interviewsSynchronizer = Create.InterviewsSynchronizer(
                readyToSendInterviewsRepositoryWriter: readyToSendInterviewsRepositoryWriter,
                interviewSummaryRepositoryWriter: interviewSummaryRepositoryWriter,
                eventStore: eventStore,
                logger: loggerMock.Object,
                jsonUtils: jsonUtils,
                httpMessageHandler: httpMessageHandler,
                headquartersPushContext: headquartersPushContextMock.Object);
        };

        Because of = () =>
            interviewsSynchronizer.Push(userId);

        It should_log_error = () =>
            loggerMock.Verify(logger => logger.Error(it.IsAny<string>(), it.IsAny<Exception>()), Times.Once);

        It should_register_error_in_push_context = () =>
            headquartersPushContextMock.Verify(context => context.PushError(it.IsAny<string>()), Times.Once);

        private static Mock<ILogger> loggerMock = new Mock<ILogger>();
        private static InterviewsSynchronizer interviewsSynchronizer;
        private static Guid userId = Guid.Parse("DDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDD");
        private static Mock<HeadquartersPushContext> headquartersPushContextMock = new Mock<HeadquartersPushContext>();
    }
}