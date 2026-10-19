using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Moq;
using NUnit.Framework;
using WB.Core.BoundedContexts.Designer.AnonymousQuestionnaires;
using WB.Core.BoundedContexts.Designer.Commands.Questionnaire;
using WB.Core.BoundedContexts.Designer.DataAccess;
using WB.Core.BoundedContexts.Designer.MembershipProvider;
using WB.Core.BoundedContexts.Designer.MembershipProvider.Roles;
using WB.Core.Infrastructure.CommandBus;
using WB.UI.Designer.Controllers;

namespace WB.Tests.Unit.Designer.Applications.QuestionnaireControllerTests
{
    [TestFixture]
    [TestOf(typeof(QuestionnaireController))]
    internal class when_managing_anonymous_sharing : QuestionnaireControllerTestContext
    {
        private static readonly Guid QuestionnaireId = Guid.Parse("11111111111111111111111111111111");
        private static readonly Guid UserId = Guid.Parse("22222222222222222222222222222222");

        [Test]
        public async Task when_enabling_not_shared_questionnaire_should_execute_enable_command()
        {
            var commandService = new Mock<ICommandService>();
            var dbContext = Create.InMemoryDbContext();
            commandService
                .Setup(x => x.Execute(It.IsAny<ICommand>(), It.IsAny<string>()))
                .Callback<ICommand, string>((_, _) => AddRecord(dbContext, isActive: true));
            var controller = CreateController(commandService.Object, dbContext);

            var result = await controller.UpdateAnonymousQuestionnaireSettings(QuestionnaireId,
                new QuestionnaireController.UpdateAnonymousQuestionnaireSettingsModel { IsActive = true });

            result.Should().BeOfType<JsonResult>();
            commandService.Verify(x => x.Execute(
                It.Is<ICommand>(c => c is EnableAnonymousSharing
                                     && ((EnableAnonymousSharing) c).QuestionnaireId == QuestionnaireId
                                     && ((EnableAnonymousSharing) c).ResponsibleId == UserId
                                     && !((EnableAnonymousSharing) c).IsResponsibleAdmin),
                It.IsAny<string>()), Times.Once);
        }

        [Test]
        public async Task when_enabling_already_enabled_questionnaire_should_not_execute_any_command()
        {
            var commandService = new Mock<ICommandService>();
            var dbContext = Create.InMemoryDbContext();
            AddRecord(dbContext, isActive: true);
            var controller = CreateController(commandService.Object, dbContext);

            await controller.UpdateAnonymousQuestionnaireSettings(QuestionnaireId,
                new QuestionnaireController.UpdateAnonymousQuestionnaireSettingsModel { IsActive = true });

            commandService.Verify(x => x.Execute(It.IsAny<ICommand>(), It.IsAny<string>()), Times.Never);
        }

        [Test]
        public async Task when_disabling_enabled_questionnaire_should_execute_disable_command()
        {
            var commandService = new Mock<ICommandService>();
            var dbContext = Create.InMemoryDbContext();
            AddRecord(dbContext, isActive: true);
            var controller = CreateController(commandService.Object, dbContext);

            await controller.UpdateAnonymousQuestionnaireSettings(QuestionnaireId,
                new QuestionnaireController.UpdateAnonymousQuestionnaireSettingsModel { IsActive = false });

            commandService.Verify(x => x.Execute(
                It.Is<ICommand>(c => c is DisableAnonymousSharing && ((DisableAnonymousSharing) c).QuestionnaireId == QuestionnaireId),
                It.IsAny<string>()), Times.Once);
        }

        [Test]
        public async Task when_regenerating_link_should_execute_regenerate_command_with_admin_flag()
        {
            var commandService = new Mock<ICommandService>();
            var dbContext = Create.InMemoryDbContext();
            AddRecord(dbContext, isActive: true);
            var controller = CreateController(commandService.Object, dbContext, isAdmin: true);

            var result = await controller.RegenerateAnonymousQuestionnaireLink(QuestionnaireId);

            result.Should().BeOfType<JsonResult>();
            commandService.Verify(x => x.Execute(
                It.Is<ICommand>(c => c is RegenerateAnonymousSharingLink
                                     && ((RegenerateAnonymousSharingLink) c).QuestionnaireId == QuestionnaireId
                                     && ((RegenerateAnonymousSharingLink) c).IsResponsibleAdmin),
                It.IsAny<string>()), Times.Once);
        }

        private static void AddRecord(DesignerDbContext dbContext, bool isActive)
        {
            dbContext.AnonymousQuestionnaires.Add(new AnonymousQuestionnaire
            {
                QuestionnaireId = QuestionnaireId,
                AnonymousQuestionnaireId = Guid.NewGuid(),
                IsActive = isActive,
                GeneratedAtUtc = DateTime.UtcNow
            });
            dbContext.SaveChanges();
        }

        private static QuestionnaireController CreateController(ICommandService commandService, DesignerDbContext dbContext, bool isAdmin = false)
        {
            var users = new Mock<UserManager<DesignerIdentityUser>>(
                Mock.Of<IUserStore<DesignerIdentityUser>>(), null, null, null, null, null, null, null, null);
            users.Setup(x => x.GetUserAsync(It.IsAny<ClaimsPrincipal>()))
                .ReturnsAsync((DesignerIdentityUser) null);

            var controller = CreateQuestionnaireController(
                commandService: commandService,
                dbContext: dbContext,
                users: users.Object);

            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, UserId.ToString()),
                new Claim(ClaimTypes.Name, "user"),
            }.ToList();
            if (isAdmin)
                claims.Add(new Claim(ClaimTypes.Role, SimpleRoleEnum.Administrator.ToString()));

            controller.ControllerContext.HttpContext.User = new ClaimsPrincipal(new ClaimsIdentity(claims, "test"));
            return controller;
        }
    }
}

