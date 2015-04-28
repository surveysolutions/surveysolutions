using System;
using System.Net.Http;
using System.Threading;
using Machine.Specifications;
using Moq;
using Moq.Protected;
using WB.Core.BoundedContexts.Supervisor.Interviews.Implementation.Views;
using WB.Core.BoundedContexts.Supervisor.Synchronization.Implementation;
using WB.Core.GenericSubdomains.Utils.Services;
using WB.Tests.Unit.SharedKernels.SurveyManagement;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.BoundedContexts.Supervisor.Synchronization.InterviewsSynchronizerTests.Push
{
    internal class when_pushing_interviews_and_no_interviews_are_ready : InterviewsSynchronizerTestsContext
    {
        Establish context = () =>
        {
            var readyToSendInterviewsRepositoryWriter = new TestInMemoryWriter<ReadyToSendToHeadquartersInterview>();

            interviewsSynchronizer = Create.InterviewsSynchronizer(
                readyToSendInterviewsRepositoryReader: readyToSendInterviewsRepositoryWriter,
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