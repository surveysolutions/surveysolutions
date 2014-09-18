using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Net.Http;
using System.Threading;
using Machine.Specifications;
using Moq;
using Moq.Protected;
using Ncqrs.Commanding;
using Ncqrs.Commanding.ServiceModel;
using Ncqrs.Eventing;
using Ncqrs.Eventing.Storage;
using WB.Core.BoundedContexts.Supervisor.Interviews.Implementation.Views;
using WB.Core.BoundedContexts.Supervisor.Synchronization.Implementation;
using WB.Core.GenericSubdomains.Logging;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernels.DataCollection.Commands.Interview;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.DataCollection.Views.BinaryData;
using It = Machine.Specifications.It;
using it = Moq.It;

namespace WB.Core.BoundedContexts.Supervisor.Tests.Synchronization.InterviewsSynchronizerTests
{
    internal class when_pushing_one_interview_with_all_events_having_hq_synchronization_origin : InterviewsSynchronizerTestsContext
    {
        Establish context = () =>
        {
            var eventStream = new CommittedEventStream(interviewId, new[]
            {
                Create.CommittedEvent(origin: "hq-sync", eventIdentifier: Guid.Parse("11111111111111111111111111111111"), eventSourceId: interviewId, eventSequence: 1),
                Create.CommittedEvent(origin: "hq-sync", eventIdentifier: Guid.Parse("22222222222222222222222222222222"), eventSourceId: interviewId, eventSequence: 2),
                Create.CommittedEvent(origin: "hq-sync", eventIdentifier: Guid.Parse("33333333333333333333333333333333"), eventSourceId: interviewId, eventSequence: 3),
                Create.CommittedEvent(origin: "hq-sync", eventIdentifier: Guid.Parse("44444444444444444444444444444444"), eventSourceId: interviewId, eventSequence: 4),
                Create.CommittedEvent(origin: "hq-sync", eventIdentifier: Guid.Parse("55555555555555555555555555555555"), eventSourceId: interviewId, eventSequence: 5),
            });

            var eventStore = Mock.Of<IEventStore>(store
                => store.ReadFrom(interviewId, 0, it.IsAny<long>()) == eventStream);

            var readyToSendInterviewsRepositoryWriter = Mock.Of<IQueryableReadSideRepositoryWriter<ReadyToSendToHeadquartersInterview>>(writer
                => writer.QueryAll(it.IsAny<Expression<Func<ReadyToSendToHeadquartersInterview, bool>>>()) == new[] { new ReadyToSendToHeadquartersInterview(interviewId) });

            fileSyncRepository.Setup(x => x.GetBinaryFilesFromSyncFolder()).Returns(new List<InterviewBinaryData>());

            interviewsSynchronizer = Create.InterviewsSynchronizer(
                readyToSendInterviewsRepositoryWriter: readyToSendInterviewsRepositoryWriter,
                httpMessageHandler: () => httpMessageHandlerMock.Object,
                eventStore: eventStore,
                logger: loggerMock.Object,
                commandService: commandServiceMock.Object,
                fileSyncRepository: fileSyncRepository.Object);
        };

        Because of = () =>
            interviewsSynchronizer.Push(userId);

        It should_not_perform_http_calls = () =>
            httpMessageHandlerMock.Protected().Verify("SendAsync", Times.Never(), ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>());

        It should_not_log_errors = () =>
            loggerMock.Verify(logger => logger.Error(it.IsAny<string>(), it.IsAny<Exception>()), Times.Never);

        It should_interview_files_be_moved_to_sync_storage = () =>
          fileSyncRepository.Verify(x=>x.MoveInterviewsBinaryDataToSyncFolder(interviewId), Times.Once);

        It should_mark_interview_as_sent_to_hq_using_hq_synchronization_origin = () =>
            commandServiceMock.Verify(service =>
                service.Execute(it.Is<MarkInterviewAsSentToHeadquarters>(command => command.InterviewId == interviewId), "hq-sync"),
                Times.Once);

        It should_execute_only_one_command = () =>
            commandServiceMock.Verify(service =>
                service.Execute(it.IsAny<ICommand>(), it.IsAny<string>()),
                Times.Once);

        private static Mock<HttpMessageHandler> httpMessageHandlerMock = new Mock<HttpMessageHandler>();
        private static Mock<IFileSyncRepository> fileSyncRepository = new Mock<IFileSyncRepository>();
        private static InterviewsSynchronizer interviewsSynchronizer;
        private static Guid userId = Guid.Parse("DDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDD");
        private static Guid interviewId = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
        private static Mock<ILogger> loggerMock = new Mock<ILogger>();
        private static Mock<ICommandService> commandServiceMock = new Mock<ICommandService>();
    }
}