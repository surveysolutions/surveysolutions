using System;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.EntityFrameworkCore;
using Moq;
using NUnit.Framework;
using WB.Core.BoundedContexts.Designer.AnonymousQuestionnaires;
using WB.Core.BoundedContexts.Designer.MembershipProvider;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.Edit;
using WB.UI.Designer.Controllers;
using WB.UI.Shared.Web.Services;

namespace WB.Tests.Unit.Designer.Applications.QuestionnaireControllerTests
{
    [TestFixture]
    [TestOf(typeof(QuestionnaireController))]
    internal class AnonymousQuestionnaireSettingsTests : QuestionnaireControllerTestContext
    {
        [TestCase(false, "user")]
        [TestCase(true, "user")]
        [TestCase(false, "link")]
        [TestCase(true, "link")]
        [TestCase(true, "questionnaire")]
        public async Task when_notification_preparation_fails_should_return_success_and_preserve_saved_settings(
            bool regenerate, string failure)
        {
            var questionnaireId = Guid.NewGuid();
            using var dbContext = Create.InMemoryDbContext();
            var previousLinkId = Guid.NewGuid();
            if (regenerate)
            {
                dbContext.AnonymousQuestionnaires.Add(new AnonymousQuestionnaire
                {
                    QuestionnaireId = questionnaireId,
                    AnonymousQuestionnaireId = previousLinkId,
                    IsActive = true,
                    GeneratedAtUtc = DateTime.UtcNow
                });
                await dbContext.SaveChangesAsync();
            }

            var userManager = CreateUserManager(new DesignerIdentityUser
                { Email = "test@example.com", UserName = "testuser" });
            var questionnaireViewFactory = CreateQuestionnaireViewFactory();
            var urlHelper = CreateUrlHelper();
            if (failure == "user")
                Mock.Get(userManager).Setup(m => m.GetUserAsync(It.IsAny<ClaimsPrincipal>()))
                    .ThrowsAsync(new InvalidOperationException("User lookup failed"));
            else if (failure == "questionnaire")
                Mock.Get(questionnaireViewFactory).Setup(f => f.Load(It.IsAny<QuestionnaireViewInputModel>()))
                    .Throws(new InvalidOperationException("Questionnaire lookup failed"));
            else
                Mock.Get(urlHelper).Setup(u => u.Action(It.IsAny<UrlActionContext>()))
                    .Throws(new InvalidOperationException("Link generation failed"));

            var emailSender = new Mock<IEmailSender>();
            var controller = CreateQuestionnaireController(
                dbContext: dbContext,
                questionnaireViewFactory: questionnaireViewFactory,
                userManager: userManager,
                emailSender: emailSender.Object);
            controller.Url = urlHelper;

            var result = regenerate
                ? await controller.RegenerateAnonymousQuestionnaireLink(questionnaireId)
                : await controller.UpdateAnonymousQuestionnaireSettings(questionnaireId,
                    new QuestionnaireController.UpdateAnonymousQuestionnaireSettingsModel { IsActive = true });

            Assert.That(result, Is.InstanceOf<JsonResult>());
            dbContext.ChangeTracker.Clear();
            var saved = await dbContext.AnonymousQuestionnaires.SingleAsync(a => a.QuestionnaireId == questionnaireId);
            Assert.That(saved.IsActive, Is.True);
            Assert.That(saved.AnonymousQuestionnaireId, Is.Not.EqualTo(previousLinkId).And.Not.EqualTo(Guid.Empty));
            emailSender.Verify(s => s.SendEmailAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()),
                Times.Never);
        }

        [Test]
        public async Task UpdateAnonymousQuestionnaireSettings_when_email_sending_fails_should_still_return_success()
        {
            var questionnaireId = Guid.NewGuid();
            var dbContext = Create.InMemoryDbContext();

            var throwingEmailSender = new Mock<IEmailSender>();
            throwingEmailSender
                .Setup(s => s.SendEmailAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                .ThrowsAsync(new Exception("SMTP connection failed"));

            var throwingViewRenderService = new Mock<IViewRenderService>();
            throwingViewRenderService
                .Setup(v => v.RenderToStringAsync(It.IsAny<string>(), It.IsAny<object>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Microsoft.AspNetCore.Routing.RouteData>()))
                .ThrowsAsync(new Exception("View render failed"));

            var user = new DesignerIdentityUser { Email = "test@example.com", UserName = "testuser" };
            var userManager = CreateUserManager(returnUser: user);

            var controller = CreateQuestionnaireController(
                dbContext: dbContext,
                questionnaireViewFactory: CreateQuestionnaireViewFactory(),
                emailSender: throwingEmailSender.Object,
                viewRenderService: throwingViewRenderService.Object,
                userManager: userManager);
            controller.Url = CreateUrlHelper();

            var result = await controller.UpdateAnonymousQuestionnaireSettings(
                questionnaireId,
                new QuestionnaireController.UpdateAnonymousQuestionnaireSettingsModel { IsActive = true });

            Assert.That(result, Is.InstanceOf<JsonResult>(), "Should return JsonResult, not an error response");
            var json = result as JsonResult;
            var isActive = json!.Value!.GetType().GetProperty("IsActive")?.GetValue(json.Value);
            Assert.That(isActive, Is.True);
            throwingViewRenderService.Verify(
                v => v.RenderToStringAsync(It.IsAny<string>(), It.IsAny<object>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Microsoft.AspNetCore.Routing.RouteData>()),
                Times.Once, "View render should have been called, confirming the email path was exercised");
        }

        [Test]
        public async Task UpdateAnonymousQuestionnaireSettings_when_questionnaire_not_found_returns_not_found()
        {
            var questionnaireId = Guid.NewGuid();
            var dbContext = Create.InMemoryDbContext();

            var controller = CreateQuestionnaireController(dbContext: dbContext);

            var result = await controller.UpdateAnonymousQuestionnaireSettings(
                questionnaireId,
                new QuestionnaireController.UpdateAnonymousQuestionnaireSettingsModel { IsActive = true });

            Assert.That(result, Is.InstanceOf<NotFoundResult>(), "Should return NotFound when questionnaire does not exist");
        }

        [Test]
        public async Task UpdateAnonymousQuestionnaireSettings_when_enabling_creates_new_record_in_db()
        {
            var questionnaireId = Guid.NewGuid();
            var dbContext = Create.InMemoryDbContext();
            var controller = CreateQuestionnaireController(
                dbContext: dbContext,
                questionnaireViewFactory: CreateQuestionnaireViewFactory());

            var result = await controller.UpdateAnonymousQuestionnaireSettings(
                questionnaireId,
                new QuestionnaireController.UpdateAnonymousQuestionnaireSettingsModel { IsActive = true });

            var json = result as JsonResult;
            Assert.That(json, Is.Not.Null);
            var isActive = json!.Value!.GetType().GetProperty("IsActive")?.GetValue(json.Value);
            Assert.That(isActive, Is.True, "Response should indicate IsActive = true");
            var anonymousQuestionnaireId = json.Value.GetType().GetProperty("AnonymousQuestionnaireId")?.GetValue(json.Value);
            Assert.That(anonymousQuestionnaireId, Is.Not.EqualTo(Guid.Empty), "Should return a valid AnonymousQuestionnaireId");
        }

        [Test]
        public async Task UpdateAnonymousQuestionnaireSettings_when_disabling_existing_record_returns_success()
        {
            var questionnaireId = Guid.NewGuid();
            var dbContext = Create.InMemoryDbContext();
            dbContext.AnonymousQuestionnaires.Add(new AnonymousQuestionnaire
            {
                QuestionnaireId = questionnaireId,
                AnonymousQuestionnaireId = Guid.NewGuid(),
                IsActive = true,
                GeneratedAtUtc = DateTime.UtcNow
            });
            await dbContext.SaveChangesAsync();
            var controller = CreateQuestionnaireController(
                dbContext: dbContext,
                questionnaireViewFactory: CreateQuestionnaireViewFactory());

            var result = await controller.UpdateAnonymousQuestionnaireSettings(
                questionnaireId,
                new QuestionnaireController.UpdateAnonymousQuestionnaireSettingsModel { IsActive = false });

            var json = result as JsonResult;
            Assert.That(json, Is.Not.Null);
            var isActive = json!.Value!.GetType().GetProperty("IsActive")?.GetValue(json.Value);
            Assert.That(isActive, Is.False, "Response should indicate IsActive = false");
        }

        [Test]
        public async Task RegenerateAnonymousQuestionnaireLink_when_email_sending_fails_should_still_return_success()
        {
            var questionnaireId = Guid.NewGuid();
            var dbContext = Create.InMemoryDbContext();
            dbContext.AnonymousQuestionnaires.Add(new AnonymousQuestionnaire
            {
                QuestionnaireId = questionnaireId,
                AnonymousQuestionnaireId = Guid.NewGuid(),
                IsActive = true,
                GeneratedAtUtc = DateTime.UtcNow
            });
            await dbContext.SaveChangesAsync();

            var throwingEmailSender = new Mock<IEmailSender>();
            throwingEmailSender
                .Setup(s => s.SendEmailAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                .ThrowsAsync(new Exception("SMTP connection failed"));

            var viewRenderService = new Mock<IViewRenderService>();
            viewRenderService
                .Setup(v => v.RenderToStringAsync(It.IsAny<string>(), It.IsAny<object>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Microsoft.AspNetCore.Routing.RouteData>()))
                .ReturnsAsync("<html>email body</html>");

            var user = new DesignerIdentityUser { Email = "test@example.com", UserName = "testuser" };
            var userManager = CreateUserManager(returnUser: user);

            var controller = CreateQuestionnaireController(
                dbContext: dbContext,
                questionnaireViewFactory: CreateQuestionnaireViewFactory(),
                emailSender: throwingEmailSender.Object,
                viewRenderService: viewRenderService.Object,
                userManager: userManager);
            controller.Url = CreateUrlHelper();

            var result = await controller.RegenerateAnonymousQuestionnaireLink(questionnaireId);

            Assert.That(result, Is.InstanceOf<JsonResult>(), "Should return JsonResult, not an error response");
            throwingEmailSender.Verify(
                s => s.SendEmailAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()),
                Times.Once, "Email sender should have been called, confirming the email path was exercised");
        }

        [Test]
        public async Task RegenerateAnonymousQuestionnaireLink_when_no_existing_record_returns_not_found()
        {
            var questionnaireId = Guid.NewGuid();
            var dbContext = Create.InMemoryDbContext();

            var controller = CreateQuestionnaireController(dbContext: dbContext);

            var result = await controller.RegenerateAnonymousQuestionnaireLink(questionnaireId);

            Assert.That(result, Is.InstanceOf<NotFoundResult>());
        }

        [Test]
        public async Task UpdateAnonymousQuestionnaireSettings_when_name_claim_is_missing_should_still_return_success()
        {
            var questionnaireId = Guid.NewGuid();
            var dbContext = Create.InMemoryDbContext();
            var emailSender = new Mock<IEmailSender>();
            var user = new DesignerIdentityUser { Email = "test@example.com", UserName = "testuser" };
            var userManager = CreateUserManager(returnUser: user);

            var controller = CreateQuestionnaireController(
                dbContext: dbContext,
                questionnaireViewFactory: CreateQuestionnaireViewFactory(),
                emailSender: emailSender.Object,
                viewRenderService: Mock.Of<IViewRenderService>(),
                userManager: userManager);
            controller.Url = CreateUrlHelper();
            controller.ControllerContext.HttpContext.User = new ClaimsPrincipal(new ClaimsIdentity());

            var result = await controller.UpdateAnonymousQuestionnaireSettings(
                questionnaireId,
                new QuestionnaireController.UpdateAnonymousQuestionnaireSettingsModel { IsActive = true });

            Assert.That(result, Is.InstanceOf<JsonResult>(), "Should return JsonResult, not an error response");
            emailSender.Verify(
                s => s.SendEmailAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()),
                Times.Once,
                "Email sender should have been called even when the name claim is missing");
        }
    }
}
