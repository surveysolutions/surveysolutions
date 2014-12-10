using System;
using System.Linq;
using System.Linq.Expressions;
using System.Net.Http;
using System.Threading;
using Machine.Specifications;
using Moq;
using Moq.Protected;
using WB.Core.BoundedContexts.Supervisor.Interviews.Implementation.Views;
using WB.Core.BoundedContexts.Supervisor.Synchronization.Implementation;
using WB.Core.GenericSubdomains.Logging;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.BoundedContexts.Supervisor.Synchronization.InterviewsSynchronizerTests.Push
{
    internal class when_pushing_interviews_and_no_interviews_are_ready : InterviewsSynchronizerTestsContext
    {
        Establish context = () =>
        {
            var readyToSendInterviewsRepositoryWriter = Mock.Of<IQueryableReadSideRepositoryWriter<ReadyToSendToHeadquartersInterview>>(writer
                => writer.QueryAll(Moq.It.IsAny<Expression<Func<ReadyToSendToHeadquartersInterview, bool>>>()) == Enumerable.Empty<ReadyToSendToHeadquartersInterview>());

            interviewsSynchronizer = Create.InterviewsSynchronizer(
                readyToSendInterviewsRepositoryWriter: readyToSendInterviewsRepositoryWriter,
                httpMessageHandler: () => httpMessageHandlerMock.Object,
                logger: loggerMock.Object);
        };

        Because of = () =>
            interviewsSynchronizer.Push(userId);

        It should_not_perform_http_calls = () =>
            httpMessageHandlerMock.Protected().Verify("SendAsync", Times.Never(), ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>());

        It should_not_log_errors = () =>
            loggerMock.Verify(logger => logger.Error(Moq.It.IsAny<string>(), Moq.It.IsAny<Exception>()), Times.Never);

        private static Mock<HttpMessageHandler> httpMessageHandlerMock = new Mock<HttpMessageHandler>();
        private static InterviewsSynchronizer interviewsSynchronizer;
        private static Guid userId = Guid.Parse("DDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDD");
        private static Mock<ILogger> loggerMock = new Mock<ILogger>();
    }
}