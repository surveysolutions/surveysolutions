using System;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Machine.Specifications;
using Main.Core.Utility;
using Moq;
using Moq.Protected;
using Ncqrs.Commanding.ServiceModel;
using Newtonsoft.Json;
using WB.Core.BoundedContexts.Supervisor.Users.Implementation;
using WB.Core.GenericSubdomains.Utils;
using WB.Core.SharedKernels.DataCollection.Commands.User;
using It = Machine.Specifications.It;

namespace WB.Core.BoundedContexts.Supervisor.Tests.Users.HeadquartersLoginServiceTests
{
    [Subject(typeof(HeadquartersLoginService))]
    public class when_valid_credentials_provided
    {
        Establish context = () =>
        {
            commandService = new Mock<ICommandService>();
            var handler = new Mock<HttpMessageHandler>();
            userId = Guid.NewGuid().FormatGuid();
            handler.Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
                .Returns(Task<HttpResponseMessage>.Factory.StartNew(() => new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent(JsonConvert.SerializeObject(new
                    {
                        isValid = true,
                        userId = userId
                    }))
                }));

            service = Create.HeadquartersLoginService(messageHandler: handler.Object,
                commandService: commandService.Object);
        };

        Because of = () => service.LoginAndCreateAccount("login", "pwd");

        It should_create_new_local_user = () => commandService.Verify(x => x.Execute(Moq.It.Is<CreateUserCommand>(
            command => 
                command.PublicKey == Guid.Parse(userId) && 
                command.Password == SimpleHash.ComputeHash("pwd") &&
                command.UserName == "login")));

        static HeadquartersLoginService service;
        static Mock<ICommandService> commandService;
        static string userId;
    }
}