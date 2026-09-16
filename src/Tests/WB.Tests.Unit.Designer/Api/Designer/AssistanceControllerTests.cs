using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using WB.Core.BoundedContexts.Designer.Implementation;
using WB.Core.BoundedContexts.Designer.MembershipProvider;
using WB.Core.BoundedContexts.Designer.Views;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.ChangeHistory;
using WB.Core.Infrastructure.PlainStorage;
using WB.UI.Designer.Code;
using WB.UI.Designer.Controllers.Api.Designer;
using WB.UI.Designer.Services;

namespace WB.Tests.Unit.Designer.Api.Designer
{
    [TestFixture]
    [TestOf(typeof(AssistanceController))]
    public class AssistanceControllerTests
    {
        private static UserManager<DesignerIdentityUser> CreateUserManager(DesignerIdentityUser returnUser = null)
        {
            var store = new Mock<IUserStore<DesignerIdentityUser>>();
            var userManagerMock = new Mock<UserManager<DesignerIdentityUser>>(
                store.Object, null, null, null, null, null, null, null, null);
            userManagerMock
                .Setup(m => m.GetUserAsync(It.IsAny<ClaimsPrincipal>()))
                .ReturnsAsync(returnUser);
            return userManagerMock.Object;
        }

        private static AssistanceController CreateController(
            IPlainKeyValueStorage<AssistantSettings> appSettingsStorage = null,
            UserManager<DesignerIdentityUser> userManager = null,
            IConfiguration configuration = null,
            IQuestionnaireHelper questionnaireHelper = null,
            IJwtTokenService jwtTokenService = null,
            IHttpClientFactory httpClientFactory = null)
        {
            var controller = new AssistanceController(
                configuration: configuration ?? Mock.Of<IConfiguration>(),
                logger: Mock.Of<ILogger<AssistanceController>>(),
                appSettingsStorage: appSettingsStorage ?? Mock.Of<IPlainKeyValueStorage<AssistantSettings>>(),
                userManager: userManager ?? CreateUserManager(),
                questionnaireHelper: questionnaireHelper ?? Mock.Of<IQuestionnaireHelper>(),
                jwtTokenService: jwtTokenService ?? Mock.Of<IJwtTokenService>(),
                httpClientFactory: httpClientFactory ?? Mock.Of<IHttpClientFactory>());

            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    User = new ClaimsPrincipal(new ClaimsIdentity(new[]
                    {
                        new Claim(ClaimTypes.NameIdentifier, Guid.NewGuid().ToString())
                    }))
                }
            };

            return controller;
        }

        private static IPlainKeyValueStorage<AssistantSettings> StorageWithSettings(AssistantSettings settings)
        {
            var storage = new TestPlainStorage<AssistantSettings>();
            storage.Store(settings, AppSetting.AssistantSettingsKey);
            return storage;
        }

        [Test]
        public async Task Post_when_settings_are_null_should_return_406()
        {
            var controller = CreateController(appSettingsStorage: new TestPlainStorage<AssistantSettings>());

            var result = await controller.Post(
                Guid.NewGuid(),
                new AssistanceController.AssistanceRequest { EntityId = Guid.NewGuid(), Prompt = "test" },
                CancellationToken.None) as ObjectResult;

            Assert.That(result.StatusCode, Is.EqualTo(406));
        }

        [Test]
        public async Task Post_when_assistant_is_disabled_should_return_406()
        {
            var controller = CreateController(
                appSettingsStorage: StorageWithSettings(new AssistantSettings { IsEnabled = false }));

            var result = await controller.Post(
                Guid.NewGuid(),
                new AssistanceController.AssistanceRequest { EntityId = Guid.NewGuid(), Prompt = "test" },
                CancellationToken.None) as ObjectResult;

            Assert.That(result.StatusCode, Is.EqualTo(406));
        }

        [Test]
        public async Task Post_when_assistant_not_enabled_for_user_should_return_406()
        {
            var controller = CreateController(
                appSettingsStorage: StorageWithSettings(new AssistantSettings
                {
                    IsEnabled = true,
                    IsAvailableToAllUsers = false
                }),
                userManager: CreateUserManager(new DesignerIdentityUser { AssistantEnabled = false }));

            var result = await controller.Post(
                Guid.NewGuid(),
                new AssistanceController.AssistanceRequest { EntityId = Guid.NewGuid(), Prompt = "test" },
                CancellationToken.None) as ObjectResult;

            Assert.That(result.StatusCode, Is.EqualTo(406));
        }

        [Test]
        public async Task Post_when_questionnaireId_is_empty_should_return_400()
        {
            var controller = CreateController(
                appSettingsStorage: StorageWithSettings(
                    new AssistantSettings { IsEnabled = true, IsAvailableToAllUsers = true }));

            var result = await controller.Post(
                Guid.Empty,
                new AssistanceController.AssistanceRequest { EntityId = Guid.NewGuid(), Prompt = "test" },
                CancellationToken.None) as BadRequestObjectResult;

            Assert.That(result, Is.Not.Null);
            Assert.That(result.StatusCode, Is.EqualTo(400));
        }

        [Test]
        public async Task Post_when_entityId_is_missing_should_return_400()
        {
            var controller = CreateController(
                appSettingsStorage: StorageWithSettings(
                    new AssistantSettings { IsEnabled = true, IsAvailableToAllUsers = true }));

            var result = await controller.Post(
                Guid.NewGuid(),
                new AssistanceController.AssistanceRequest { Prompt = "test" },
                CancellationToken.None) as BadRequestObjectResult;

            Assert.That(result, Is.Not.Null);
            Assert.That(result.StatusCode, Is.EqualTo(400));
        }

        [Test]
        public async Task Post_when_prompt_and_messages_are_both_empty_should_return_400()
        {
            var controller = CreateController(
                appSettingsStorage: StorageWithSettings(
                    new AssistantSettings { IsEnabled = true, IsAvailableToAllUsers = true }));

            var result = await controller.Post(
                Guid.NewGuid(),
                new AssistanceController.AssistanceRequest { EntityId = Guid.NewGuid(), Prompt = "" },
                CancellationToken.None) as BadRequestObjectResult;

            Assert.That(result, Is.Not.Null);
            Assert.That(result.StatusCode, Is.EqualTo(400));
        }

        [Test]
        public async Task Post_when_conversationId_is_supplied_should_forward_it_and_not_send_messages()
        {
            var questionnaireId = Guid.NewGuid();
            var entityId = Guid.NewGuid();
            var conversationId = Guid.NewGuid();
            var assistantAddress = "https://assistant.example/api/assist";

            var questionnaireHelper = new Mock<IQuestionnaireHelper>();
            questionnaireHelper
                .Setup(x => x.GetLastRevision(questionnaireId))
                .Returns(new QuestionnaireRevision(questionnaireId, version: 7));

            var messageHandler = new CapturingMessageHandler(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("{}", Encoding.UTF8, "application/json")
            });

            var httpClientFactory = new Mock<IHttpClientFactory>();
            httpClientFactory
                .Setup(x => x.CreateClient("AssistantProvider"))
                .Returns(new HttpClient(messageHandler));

            var configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string>
                {
                    ["Providers:Assistant:AssistantAddress"] = assistantAddress
                })
                .Build();

            var controller = CreateController(
                appSettingsStorage: StorageWithSettings(
                    new AssistantSettings { IsEnabled = true, IsAvailableToAllUsers = true }),
                configuration: configuration,
                questionnaireHelper: questionnaireHelper.Object,
                httpClientFactory: httpClientFactory.Object);

            var result = await controller.Post(
                questionnaireId,
                new AssistanceController.AssistanceRequest
                {
                    EntityId = entityId,
                    Prompt = "continue from earlier",
                    ConversationId = conversationId
                },
                CancellationToken.None) as OkObjectResult;

            using var requestBody = JsonDocument.Parse(messageHandler.LastRequestBody);
            var payload = requestBody.RootElement;

            Assert.Multiple(() =>
            {
                Assert.That(result, Is.Not.Null);
                Assert.That(messageHandler.LastRequest, Is.Not.Null);
                Assert.That(messageHandler.LastRequest!.RequestUri, Is.EqualTo(new Uri(assistantAddress)));
                Assert.That(payload.GetProperty("Prompt").GetString(), Is.EqualTo("continue from earlier"));
                Assert.That(payload.GetProperty("conversationId").GetGuid(), Is.EqualTo(conversationId));
                Assert.That(payload.TryGetProperty("Messages", out _), Is.False);
                Assert.That(payload.TryGetProperty("messages", out _), Is.False);
            });
        }

        [Test]
        public async Task Reaction_when_settings_are_null_should_return_406()
        {
            var controller = CreateController(appSettingsStorage: new TestPlainStorage<AssistantSettings>());

            var result = await controller.Reaction(
                Guid.NewGuid(),
                new AssistanceController.AssistanceReactionRequest
                {
                    EntityId = Guid.NewGuid(),
                    AssistantResponse = "response",
                    AssistantCallId = 1
                },
                CancellationToken.None) as ObjectResult;

            Assert.That(result.StatusCode, Is.EqualTo(406));
        }

        [Test]
        public async Task Reaction_when_entityId_is_missing_should_return_400()
        {
            var controller = CreateController(
                appSettingsStorage: StorageWithSettings(
                    new AssistantSettings { IsEnabled = true, IsAvailableToAllUsers = true }));

            var result = await controller.Reaction(
                Guid.NewGuid(),
                new AssistanceController.AssistanceReactionRequest
                {
                    AssistantResponse = "response",
                    AssistantCallId = 1
                },
                CancellationToken.None) as BadRequestObjectResult;

            Assert.That(result, Is.Not.Null);
            Assert.That(result.StatusCode, Is.EqualTo(400));
        }

        [Test]
        public async Task Reaction_when_assistantCallId_is_missing_should_return_400()
        {
            var controller = CreateController(
                appSettingsStorage: StorageWithSettings(
                    new AssistantSettings { IsEnabled = true, IsAvailableToAllUsers = true }));

            var result = await controller.Reaction(
                Guid.NewGuid(),
                new AssistanceController.AssistanceReactionRequest
                {
                    EntityId = Guid.NewGuid(),
                    AssistantResponse = "response"
                },
                CancellationToken.None) as BadRequestObjectResult;

            Assert.That(result, Is.Not.Null);
            Assert.That(result.StatusCode, Is.EqualTo(400));
        }

        private class CapturingMessageHandler : HttpMessageHandler
        {
            private readonly HttpResponseMessage response;

            public CapturingMessageHandler(HttpResponseMessage response)
            {
                this.response = response;
            }

            public HttpRequestMessage LastRequest { get; private set; }
            public string LastRequestBody { get; private set; }

            protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
            {
                this.LastRequest = request;
                this.LastRequestBody = request.Content != null
                    ? await request.Content.ReadAsStringAsync(cancellationToken)
                    : null;
                return this.response;
            }
        }
    }
}
