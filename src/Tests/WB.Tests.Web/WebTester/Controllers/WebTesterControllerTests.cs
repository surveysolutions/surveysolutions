#nullable enable
using System;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using NUnit.Framework;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.UI.WebTester.Controllers;
using WB.UI.WebTester.Services;
using WB.UI.WebTester.Services.Implementation;

namespace WB.Tests.Web.WebTester.Controllers
{
    [TestFixture]
    [TestOf(typeof(WebTesterController))]
    internal class WebTesterControllerTests
    {
        [Test]
        public async Task when_exchanging_code_should_redirect_with_the_created_run_id()
        {
            var questionnaireId = Guid.NewGuid();
            var createdInterviewId = Guid.Empty;
            var sessionService = new Mock<IWebTesterSessionService>();
            sessionService
                .Setup(x => x.AuthorizeQuestionnaire(It.IsAny<ISession>(), It.IsAny<Guid>(), questionnaireId))
                .Callback<ISession, Guid, Guid>((_, interviewId, _) => createdInterviewId = interviewId);

            var codeExchangeClient = new Mock<ICodeExchangeClient>();
            codeExchangeClient
                .Setup(x => x.ExchangeAsync("code", default))
                .ReturnsAsync(new ExchangeCodeResponse
                {
                    AccessToken = "token",
                    ExpiresIn = 600,
                    QuestionnaireId = questionnaireId.ToString()
                });

            var controller = CreateController(sessionService: sessionService.Object, codeExchangeClient: codeExchangeClient.Object);

            var result = await controller.Run(questionnaireId, null, code: "code") as RedirectToActionResult;

            result.Should().NotBeNull();
            result!.RouteValues!["runId"].Should().Be(createdInterviewId);
            createdInterviewId.Should().NotBe(Guid.Empty);
        }

        [Test]
        public async Task when_run_id_is_provided_should_import_that_authorized_run()
        {
            var questionnaireId = Guid.NewGuid();
            var runId = Guid.NewGuid();
            var otherInterviewId = Guid.NewGuid();
            var sessionService = new Mock<IWebTesterSessionService>();
            sessionService.Setup(x => x.GetQuestionnaireId(It.IsAny<ISession>(), runId)).Returns(questionnaireId);
            sessionService.Setup(x => x.GetInterviewId(It.IsAny<ISession>(), questionnaireId)).Returns(otherInterviewId);
            sessionService.Setup(x => x.IsAuthorized(It.IsAny<ISession>(), runId)).Returns(true);

            var jwtStore = new Mock<IWebTesterJwtStore>();
            jwtStore.Setup(x => x.GetToken(runId)).Returns("token");

            var interviewFactory = new Mock<IImportQuestionnaireAndCreateInterviewService>();
            interviewFactory
                .Setup(x => x.StartImportQuestionnaireAndCreateInterview(questionnaireId, runId, null, null))
                .Returns(runId);

            var controller = CreateController(
                sessionService: sessionService.Object,
                jwtStore: jwtStore.Object,
                interviewFactory: interviewFactory.Object);

            var result = await controller.Run(questionnaireId, null, runId: runId);

            result.Should().BeOfType<ViewResult>();
            interviewFactory.Verify(
                x => x.StartImportQuestionnaireAndCreateInterview(questionnaireId, runId, null, null),
                Times.Once);
            sessionService.Verify(x => x.GetInterviewId(It.IsAny<ISession>(), questionnaireId), Times.Never);
        }

        [TestCase(false, true, true)]
        [TestCase(true, false, true)]
        [TestCase(true, true, false)]
        public async Task when_run_id_is_not_mapped_or_authorized_or_has_no_token_should_not_import(
            bool mapped, bool authorized, bool hasToken)
        {
            var questionnaireId = Guid.NewGuid();
            var runId = Guid.NewGuid();
            var sessionService = new Mock<IWebTesterSessionService>();
            sessionService
                .Setup(x => x.GetQuestionnaireId(It.IsAny<ISession>(), runId))
                .Returns(mapped ? questionnaireId : Guid.NewGuid());
            sessionService.Setup(x => x.IsAuthorized(It.IsAny<ISession>(), runId)).Returns(authorized);

            var jwtStore = new Mock<IWebTesterJwtStore>();
            jwtStore.Setup(x => x.GetToken(runId)).Returns(hasToken ? "token" : null);

            var interviewFactory = new Mock<IImportQuestionnaireAndCreateInterviewService>();
            var controller = CreateController(
                sessionService: sessionService.Object,
                jwtStore: jwtStore.Object,
                interviewFactory: interviewFactory.Object);

            var result = await controller.Run(questionnaireId, null, runId: runId) as RedirectToActionResult;

            result.Should().NotBeNull();
            result!.ControllerName.Should().Be("Error");
            interviewFactory.Verify(
                x => x.StartImportQuestionnaireAndCreateInterview(
                    It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<Guid?>(), It.IsAny<int?>()),
                Times.Never);
        }

        private static WebTesterController CreateController(
            IWebTesterSessionService? sessionService = null,
            IWebTesterJwtStore? jwtStore = null,
            ICodeExchangeClient? codeExchangeClient = null,
            IImportQuestionnaireAndCreateInterviewService? interviewFactory = null)
        {
            var httpContext = new DefaultHttpContext { Session = Mock.Of<ISession>() };
            return new WebTesterController(
                Mock.Of<IStatefulInterviewRepository>(),
                interviewFactory ?? Mock.Of<IImportQuestionnaireAndCreateInterviewService>(),
                Options.Create(new TesterConfiguration()),
                jwtStore ?? Mock.Of<IWebTesterJwtStore>(),
                codeExchangeClient ?? Mock.Of<ICodeExchangeClient>(),
                Mock.Of<IUserContextStore>(),
                sessionService ?? Mock.Of<IWebTesterSessionService>(),
                Mock.Of<ILogger<WebTesterController>>())
            {
                ControllerContext = new ControllerContext { HttpContext = httpContext }
            };
        }
    }
}
