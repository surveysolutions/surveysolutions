using System;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Machine.Specifications;
using Moq;
using Moq.Protected;
using Ncqrs.Commanding.ServiceModel;
using Newtonsoft.Json;
using WB.Core.BoundedContexts.Supervisor.Users.Implementation;
using WB.Core.GenericSubdomains.Logging;
using WB.Core.GenericSubdomains.Utils;
using WB.Core.SharedKernels.DataCollection.Commands.User;
using Web.Supervisor.Utils;
using It = Machine.Specifications.It;

namespace WB.Core.BoundedContexts.Supervisor.Tests.Users.HeadquartersLoginServiceTests
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

            service = Create.HeadquartersLoginService(messageHandler: handler.Object,
                commandService: commandService.Object,
                logger: logger.Object);
        };

        Because of = () => service.LoginAndCreateAccount("login", "pwd");

        It should_not_create_new_local_user = () => commandService.Verify(x => x.Execute(Moq.It.IsAny<CreateUserCommand>()), Times.Never);

        It should_log_error_message = () => logger.Verify(x => x.Error(Moq.It.Is<string>(arg => arg.StartsWith("Failed to login user")), null));

        static HeadquartersLoginService service;
        static Mock<ICommandService> commandService;
        private static Mock<ILogger> logger;
    }
}