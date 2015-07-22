using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Machine.Specifications;
using Moq;
using Moq.Protected;

using WB.Core.BoundedContexts.Supervisor.Users.Implementation;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.SharedKernels.DataCollection.Commands.User;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.BoundedContexts.Supervisor.Users.HeadquartersLoginServiceTests
{
    public class when_hq_service_fails
    {
        Establish context = () =>
        {
            commandService = new Mock<ICommandService>();
            logger = new Mock<ILogger>();

            var handler = new Mock<HttpMessageHandler>();
            handler.Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
                .Returns(Task<HttpResponseMessage>.Factory.StartNew(() => new HttpResponseMessage(HttpStatusCode.InternalServerError)
                {
                    Content = new StringContent("exception")
                }));

            service = Create.HeadquartersLoginService(messageHandler: () => handler.Object,
                logger: logger.Object, commandService: commandService.Object);
        };

        Because of = () => service.LoginAndCreateAccount("login", "pwd").Wait();

        It should_not_create_new_local_user = () => commandService.Verify(x => x.Execute(Moq.It.IsAny<CreateUserCommand>(), Moq.It.IsAny<string>(), Moq.It.IsAny<bool>()), Times.Never);

        It should_log_error_message = () => logger.Verify(x => x.Error(Moq.It.Is<string>(arg => arg.StartsWith("Failed to login user")), null));

        static HeadquartersLoginService service;
        static Mock<ICommandService> commandService;
        private static Mock<ILogger> logger;
    }
}