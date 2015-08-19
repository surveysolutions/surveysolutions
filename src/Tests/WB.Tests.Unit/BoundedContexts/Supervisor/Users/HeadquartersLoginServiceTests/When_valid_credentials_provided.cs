using System;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Machine.Specifications;
using Moq;
using Moq.Protected;
using Newtonsoft.Json;
using WB.Core.BoundedContexts.Supervisor.Users;
using WB.Core.BoundedContexts.Supervisor.Users.Implementation;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.SharedKernels.DataCollection.Commands.User;
using WB.Core.SharedKernels.SurveyManagement.Views.User;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.BoundedContexts.Supervisor.Users.HeadquartersLoginServiceTests
{
    [Subject(typeof(HeadquartersLoginService))]
    public class when_valid_credentials_provided
    {
        private const string UserDetailsUri = "http://localhost/userDetails";

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
                        userId = userId,
                        userDetailsUrl = UserDetailsUri
                    }))
                }));

            HeadquartersUser = new UserView
            {
                PublicKey = Guid.NewGuid(),
                Email = "test@test.com",
                IsLockedBySupervisor = false,
                IsLockedByHQ = false,
                Password = "password"
            };

            var userReader = new Mock<IHeadquartersUserReader>();
            userReader.Setup(x => x.GetUserByUri(new Uri(UserDetailsUri)))
                .ReturnsAsync(HeadquartersUser);

            service = Create.HeadquartersLoginService(headquartersUserReader: userReader.Object, messageHandler: () => handler.Object,
                commandService: commandService.Object);
        };

        Because of = () => service.LoginAndCreateAccount("login", "pwd").Wait();

        It should_create_new_local_user = () => commandService.Verify(x => x.Execute(
            Moq.It.Is<CreateUserCommand>(command => 
                command.PublicKey == HeadquartersUser.PublicKey && 
                command.Password == HeadquartersUser.Password &&
                command.UserName == HeadquartersUser.UserName &&
                command.Email == HeadquartersUser.Email &&
                command.IsLockedByHQ == HeadquartersUser.IsLockedByHQ &&
                command.IsLockedBySupervisor == HeadquartersUser.IsLockedBySupervisor),
            Moq.It.IsAny<string>()));

        static HeadquartersLoginService service;
        static Mock<ICommandService> commandService;
        static string userId;
        private static UserView HeadquartersUser;
    }
}