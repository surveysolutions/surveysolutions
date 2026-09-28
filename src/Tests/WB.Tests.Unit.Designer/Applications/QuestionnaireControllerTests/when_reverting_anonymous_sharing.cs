using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using NUnit.Framework;
using WB.Core.BoundedContexts.Designer.AnonymousQuestionnaires;
using WB.Core.BoundedContexts.Designer.Commands.Questionnaire;
using WB.Core.BoundedContexts.Designer.Services;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.ChangeHistory;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.Edit;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.Infrastructure.CommandBus;
using WB.UI.Designer.Controllers;

namespace WB.Tests.Unit.Designer.Applications.QuestionnaireControllerTests
{
    [TestFixture]
    [TestOf(typeof(QuestionnaireController))]
    internal class when_reverting_anonymous_sharing : QuestionnaireControllerTestContext
    {
        [Test]
        public async Task should_restore_selected_anonymous_sharing_state_without_dispatching_questionnaire_revert_command()
        {
            var questionnaireId = Guid.NewGuid();
            var historyRecordId = Guid.NewGuid();
            var userId = Guid.NewGuid();

            var dbContext = Create.InMemoryDbContext();
            dbContext.AnonymousQuestionnaires.Add(new AnonymousQuestionnaire
            {
                QuestionnaireId = questionnaireId,
                AnonymousQuestionnaireId = Guid.NewGuid(),
                IsActive = false,
                GeneratedAtUtc = DateTime.UtcNow
            });
            dbContext.QuestionnaireChangeRecords.Add(Create.QuestionnaireChangeRecord(
                questionnaireChangeRecordId: historyRecordId.FormatGuid(),
                questionnaireId: questionnaireId.FormatGuid(),
                action: QuestionnaireActionType.AnonymousSharingEnabled,
                targetId: questionnaireId,
                targetType: QuestionnaireItemType.Questionnaire,
                targetTitle: "Questionnaire title"));
            await dbContext.SaveChangesAsync();

            var commandService = new Mock<ICommandService>();
            var historyService = new Mock<IQuestionnaireHistoryVersionsService>();
            var questionnaireViewFactory = new Mock<IQuestionnaireViewFactory>();
            questionnaireViewFactory
                .Setup(x => x.HasUserChangeAccessToQuestionnaire(questionnaireId, userId))
                .Returns(true);

            var controller = CreateQuestionnaireController(
                commandService: commandService.Object,
                questionnaireViewFactory: questionnaireViewFactory.Object,
                questionnaireHistoryVersionsService: historyService.Object,
                dbContext: dbContext);
            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    User = new ClaimsPrincipal(new ClaimsIdentity(new[]
                    {
                        new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
                        new Claim(ClaimTypes.Name, "designer-user")
                    }))
                }
            };

            var result = await controller.Revert(questionnaireId, historyRecordId);

            dbContext.AnonymousQuestionnaires.Single(a => a.QuestionnaireId == questionnaireId).IsActive.Should().BeTrue();
            historyService.Verify(x => x.AddQuestionnaireChangeItemToContextAsync(
                    dbContext,
                    questionnaireId,
                    userId,
                    "designer-user",
                    QuestionnaireActionType.AnonymousSharingEnabled,
                    QuestionnaireItemType.Questionnaire,
                    questionnaireId,
                    "Questionnaire title",
                    null,
                    null,
                    null,
                    null,
                    null,
                    null),
                Times.Once);
            commandService.Verify(x => x.Execute(It.IsAny<RevertVersionQuestionnaire>()), Times.Never);
            result.Should().BeOfType<RedirectResult>()
                .Which.Url.Should().Be($"/q/details/{questionnaireId.FormatGuid()}");
        }

        [Test]
        public async Task should_not_append_history_when_anonymous_sharing_is_already_in_requested_state()
        {
            var questionnaireId = Guid.NewGuid();
            var historyRecordId = Guid.NewGuid();
            var userId = Guid.NewGuid();

            var dbContext = Create.InMemoryDbContext();
            dbContext.AnonymousQuestionnaires.Add(new AnonymousQuestionnaire
            {
                QuestionnaireId = questionnaireId,
                AnonymousQuestionnaireId = Guid.NewGuid(),
                IsActive = true,
                GeneratedAtUtc = DateTime.UtcNow
            });
            dbContext.QuestionnaireChangeRecords.Add(Create.QuestionnaireChangeRecord(
                questionnaireChangeRecordId: historyRecordId.FormatGuid(),
                questionnaireId: questionnaireId.FormatGuid(),
                action: QuestionnaireActionType.AnonymousSharingEnabled,
                targetId: questionnaireId,
                targetType: QuestionnaireItemType.Questionnaire,
                targetTitle: "Questionnaire title"));
            await dbContext.SaveChangesAsync();

            var historyService = new Mock<IQuestionnaireHistoryVersionsService>();
            var questionnaireViewFactory = new Mock<IQuestionnaireViewFactory>();
            questionnaireViewFactory
                .Setup(x => x.HasUserChangeAccessToQuestionnaire(questionnaireId, userId))
                .Returns(true);

            var controller = CreateQuestionnaireController(
                questionnaireViewFactory: questionnaireViewFactory.Object,
                questionnaireHistoryVersionsService: historyService.Object,
                dbContext: dbContext);
            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    User = new ClaimsPrincipal(new ClaimsIdentity(new[]
                    {
                        new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
                        new Claim(ClaimTypes.Name, "designer-user")
                    }))
                }
            };

            await controller.Revert(questionnaireId, historyRecordId);

            historyService.Verify(x => x.AddQuestionnaireChangeItemToContextAsync(
                    It.IsAny<WB.Core.BoundedContexts.Designer.DataAccess.DesignerDbContext>(),
                    It.IsAny<Guid>(),
                    It.IsAny<Guid>(),
                    It.IsAny<string>(),
                    It.IsAny<QuestionnaireActionType>(),
                    It.IsAny<QuestionnaireItemType>(),
                    It.IsAny<Guid>(),
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<int?>(),
                    It.IsAny<DateTime?>(),
                    It.IsAny<Main.Core.Documents.QuestionnaireDocument>(),
                    It.IsAny<QuestionnaireChangeReference>(),
                    It.IsAny<QuestionnaireChangeRecordMetadata>()),
                Times.Never);
        }
    }
}
