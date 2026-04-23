using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Moq;
using NUnit.Framework;
using WB.Core.BoundedContexts.Designer.AnonymousQuestionnaires;
using WB.Core.BoundedContexts.Designer.MembershipProvider;
using WB.UI.Designer.Controllers;
using WB.UI.Shared.Web.Services;

namespace WB.Tests.Unit.Designer.Applications.QuestionnaireControllerTests
{
    [TestFixture]
    [TestOf(typeof(QuestionnaireController))]
    internal class AnonymousQuestionnaireSettingsTests : QuestionnaireControllerTestContext
    {
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

            var result = await controller.UpdateAnonymousQuestionnaireSettings(
                questionnaireId,
                new QuestionnaireController.UpdateAnonymousQuestionnaireSettingsModel { IsActive = true });

            Assert.That(result, Is.InstanceOf<JsonResult>(), "Should return JsonResult, not an error response");
            var json = result as JsonResult;
            var isActive = json!.Value!.GetType().GetProperty("IsActive")?.GetValue(json.Value);
            Assert.That(isActive, Is.True);
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

            var user = new DesignerIdentityUser { Email = "test@example.com", UserName = "testuser" };
            var userManager = CreateUserManager(returnUser: user);

            var controller = CreateQuestionnaireController(
                dbContext: dbContext,
                questionnaireViewFactory: CreateQuestionnaireViewFactory(),
                emailSender: throwingEmailSender.Object,
                userManager: userManager);

            var result = await controller.RegenerateAnonymousQuestionnaireLink(questionnaireId);

            Assert.That(result, Is.InstanceOf<JsonResult>(), "Should return JsonResult, not an error response");
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
    }
}
