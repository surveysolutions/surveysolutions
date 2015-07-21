using System;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Machine.Specifications;
using Moq;
using Moq.Protected;

using Newtonsoft.Json;
using WB.Core.BoundedContexts.Supervisor.Users.Implementation;
using WB.Core.GenericSubdomains.Utils;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.SharedKernels.DataCollection.Commands.User;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.BoundedContexts.Supervisor.Users.HeadquartersLoginServiceTests
{
    public class when_invalid_credentials_provided
    {
        Establish context = () =>
        {
            commandService = new Mock<ICommandService>();
            var handler = new Mock<HttpMessageHandler>();
            handler.Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
                .Returns(Task<HttpResponseMessage>.Factory.StartNew(() => new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent(JsonConvert.SerializeObject(new
                    {
                        isValid = false
                    }))
                }));

            service = Create.HeadquartersLoginService(messageHandler: () => handler.Object,
                commandService: commandService.Object);
        };

        Because of = () => service.LoginAndCreateAccount("login", "pwd").Wait();

        It should_not_create_new_local_user = () => commandService.Verify(x => x.Execute(Moq.It.IsAny<CreateUserCommand>(), Moq.It.IsAny<string>(), Moq.It.IsAny<bool>()), Times.Never);

        static HeadquartersLoginService service;
        static Mock<ICommandService> commandService;
    }
}