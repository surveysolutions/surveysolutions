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
using WB.Core.GenericSubdomains.Portable;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernel.Structures.Synchronization;
using WB.Core.SharedKernels.SurveyManagement.Views.Interview;
using WB.Tests.Unit.SharedKernels.SurveyManagement;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.BoundedContexts.Supervisor.Synchronization.InterviewsSynchronizerTests.Push
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

            var readyToSendInterviewsRepositoryWriter = Stub.ReadSideRepository<ReadyToSendToHeadquartersInterview>();
            readyToSendInterviewsRepositoryWriter.Store(new ReadyToSendToHeadquartersInterview(interviewId), interviewId);

            CommittedEvent interviewEvent = Create.CommittedEvent(eventSourceId: interviewId, origin: null);
            var eventStore = Mock.Of<IEventStore>(store
                => store.ReadFrom(interviewId, 0, Moq.It.IsAny<int>()) == new CommittedEventStream(interviewId, interviewEvent));

            var interviewSummaryRepositoryWriter = Mock.Of<IReadSideRepositoryReader<InterviewSummary>>(writer
                => writer.GetById(interviewId.FormatGuid()) == Create.InterviewSummary());

            var jsonUtils = Mock.Of<IJsonUtils>(utils
                => utils.Serialize(Moq.It.IsAny<InterviewMetaInfo>(), TypeSerializationSettings.ObjectsOnly) == "metadata json"
                && utils.Serialize(Moq.It.IsAny<AggregateRootEvent[]>(), TypeSerializationSettings.ObjectsOnly) == "events json"
                && utils.Serialize(Moq.It.IsAny<SyncItem>(), TypeSerializationSettings.ObjectsOnly) == "sync item json"
                && utils.Deserialize<bool>(negativeResponse) == false);

            interviewsSynchronizer = Create.InterviewsSynchronizer(
                readyToSendInterviewsRepositoryReader: readyToSendInterviewsRepositoryWriter,
                interviewSummaryRepositoryReader: interviewSummaryRepositoryWriter,
                eventStore: eventStore,
                logger: loggerMock.Object,
                jsonUtils: jsonUtils,
                httpMessageHandler: ()=> httpMessageHandler,
                headquartersPushContext: headquartersPushContextMock.Object);
        };

        Because of = () =>
            interviewsSynchronizer.Push(userId);

        It should_log_error = () =>
            loggerMock.Verify(logger => logger.Error(Moq.It.IsAny<string>(), Moq.It.IsAny<Exception>()), Times.Once);

        It should_register_error_in_push_context = () =>
            headquartersPushContextMock.Verify(context => context.PushError(Moq.It.IsAny<string>()), Times.Once);

        private static Mock<ILogger> loggerMock = new Mock<ILogger>();
        private static InterviewsSynchronizer interviewsSynchronizer;
        private static Guid userId = Guid.Parse("DDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDD");
        private static Mock<HeadquartersPushContext> headquartersPushContextMock = new Mock<HeadquartersPushContext>();
    }
}