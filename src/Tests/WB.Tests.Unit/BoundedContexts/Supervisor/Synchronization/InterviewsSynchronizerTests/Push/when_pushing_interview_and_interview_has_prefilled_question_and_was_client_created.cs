using System;
using System.Collections.Generic;
using System.Linq;
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
using WB.Core.BoundedContexts.Supervisor.Synchronization.Implementation;
using WB.Core.GenericSubdomains.Utils;
using WB.Core.GenericSubdomains.Utils.Services;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernel.Structures.Synchronization;
using WB.Core.SharedKernels.SurveyManagement.Views.Interview;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.BoundedContexts.Supervisor.Synchronization.InterviewsSynchronizerTests.Push
{
    internal class when_pushing_interview_and_interview_has_prefilled_question_and_was_client_created : InterviewsSynchronizerTestsContext
    {
        Establish context = () =>
        {
            string positiveResponse = ":)";

            loggerMock
                .Setup(logger => logger.Error(Moq.It.IsAny<string>(), Moq.It.IsAny<Exception>()))
                .Callback<string, Exception>((error, exception) => lastLoggedException = exception);

            var httpMessageHandler = Mock.Of<HttpMessageHandler>();

            Mock.Get(httpMessageHandler)
                .Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
                .Returns(Task<HttpResponseMessage>.Factory.StartNew(() =>
                    new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent(positiveResponse) }))
                .Callback<HttpRequestMessage, CancellationToken>((message, token) =>
                    contentSentToHq = message.Content.ReadAsStringAsync().Result);

            var readyToSendInterviewsRepositoryWriter =
                Mock.Of<IQueryableReadSideRepositoryReader<ReadyToSendToHeadquartersInterview>>(writer
                    =>
                    writer.QueryAll(Moq.It.IsAny<Expression<Func<ReadyToSendToHeadquartersInterview, bool>>>()) ==
                        new[] { new ReadyToSendToHeadquartersInterview(interviewId) });

            interviewEvent = Create.CommittedEvent(eventSourceId: interviewId, origin: null);
            var eventStore = Mock.Of<IEventStore>(store
                => store.ReadFrom(interviewId, 0, Moq.It.IsAny<long>()) == new CommittedEventStream(interviewId, interviewEvent));

            InterviewSummary interviewSummary = Create.InterviewSummary();
            interviewSummary.WasCreatedOnClient = true;
            var answersToFeaturedQuestions = new Dictionary<Guid,QuestionAnswer>();
            answersToFeaturedQuestions.Add(questionId,
                new QuestionAnswer() { Answer = questionAnswer, Id = answerId, Title = questionTitle });
            interviewSummary.AnswersToFeaturedQuestions = answersToFeaturedQuestions; 

            var interviewSummaryRepositoryWriter = Mock.Of<IReadSideRepositoryWriter<InterviewSummary>>(writer
                => writer.GetById(interviewId.FormatGuid()) == interviewSummary);

            var jsonUtils = Mock.Of<IJsonUtils>(utils
                => utils.Deserialize<bool>(positiveResponse) == true);

            Mock.Get(jsonUtils)
                .Setup(utils => utils.Serialize(Moq.It.IsAny<AggregateRootEvent[]>()))
                .Returns(eventsJson)
                .Callback<object>(entity => events = (AggregateRootEvent[]) entity);

            Mock.Get(jsonUtils)
                .Setup(utils => utils.Serialize(Moq.It.IsAny<InterviewMetaInfo>()))
                .Returns(metadataJson)
                .Callback<object>(entity => metadata = (InterviewMetaInfo) entity);

            Mock.Get(jsonUtils)
                .Setup(utils => utils.Serialize(Moq.It.IsAny<SyncItem>()))
                .Returns(syncItemJson)
                .Callback<object>(entity => syncItem = (SyncItem) entity);

            interviewsSynchronizer = Create.InterviewsSynchronizer(
                readyToSendInterviewsRepositoryWriter: readyToSendInterviewsRepositoryWriter,
                interviewSummaryRepositoryWriter: interviewSummaryRepositoryWriter,
                eventStore: eventStore,
                logger: loggerMock.Object,
                jsonUtils: jsonUtils,
                httpMessageHandler: () => httpMessageHandler,
                commandService: commandServiceMock.Object);
        };

        Because of = () =>
            interviewsSynchronizer.Push(userId);


        It should_set_metadata_featuredquestions_meta_not_empty = () =>
            metadata.FeaturedQuestionsMeta.Count().ShouldEqual(1);

        It should_set_metadata_featuredquestion_public_key_to_questionId = () =>
            metadata.FeaturedQuestionsMeta.Single().PublicKey.ShouldEqual(questionId);

        It should_set_metadata_featuredquestion_title_to_question_title = () =>
            metadata.FeaturedQuestionsMeta.Single().Title.ShouldEqual(questionTitle);

        It should_set_metadata_featuredquestion_value_to_question_answer = () =>
            metadata.FeaturedQuestionsMeta.Single().Value.ShouldEqual(questionAnswer);


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
        private static Mock<ICommandService> commandServiceMock = new Mock<ICommandService>();

        private static Guid questionId = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAB");
        private static Guid answerId = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAc");
        private static string questionTitle = "title";
        private static string questionAnswer = "answer";
    }
}