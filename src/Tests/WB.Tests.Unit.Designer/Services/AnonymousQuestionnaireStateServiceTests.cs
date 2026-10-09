using System;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using WB.Core.BoundedContexts.Designer.AnonymousQuestionnaires;
using WB.Core.BoundedContexts.Designer.Implementation.Services;
using WB.Core.BoundedContexts.Designer.Services;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.ChangeHistory;

namespace WB.Tests.Unit.Designer.Services
{
    [TestFixture]
    [TestOf(typeof(AnonymousQuestionnaireStateService))]
    public class AnonymousQuestionnaireStateServiceTests
    {
        [Test]
        public async Task when_state_changes_should_append_history_and_persist_requested_value()
        {
            var questionnaireId = Guid.NewGuid();
            var responsibleId = Guid.NewGuid();
            var dbContext = Create.InMemoryDbContext();
            dbContext.AnonymousQuestionnaires.Add(new AnonymousQuestionnaire
            {
                QuestionnaireId = questionnaireId,
                AnonymousQuestionnaireId = Guid.NewGuid(),
                IsActive = false,
                GeneratedAtUtc = DateTime.UtcNow
            });
            await dbContext.SaveChangesAsync();

            var historyMutationService = new Mock<IQuestionnaireHistoryMutationService>();
            var service = new AnonymousQuestionnaireStateService(dbContext, historyMutationService.Object);

            var result = await service.SaveStateAsync(questionnaireId, true, "Questionnaire title", responsibleId, "designer-user");

            result.IsActive.Should().BeTrue();
            dbContext.AnonymousQuestionnaires.Single(x => x.QuestionnaireId == questionnaireId).IsActive.Should().BeTrue();
            historyMutationService.Verify(x => x.StageQuestionnaireChangeItemAsync(
                    questionnaireId,
                    responsibleId,
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
        }

        [Test]
        public async Task when_state_is_already_requested_value_should_not_append_history()
        {
            var questionnaireId = Guid.NewGuid();
            var responsibleId = Guid.NewGuid();
            var dbContext = Create.InMemoryDbContext();
            dbContext.AnonymousQuestionnaires.Add(new AnonymousQuestionnaire
            {
                QuestionnaireId = questionnaireId,
                AnonymousQuestionnaireId = Guid.NewGuid(),
                IsActive = true,
                GeneratedAtUtc = DateTime.UtcNow
            });
            await dbContext.SaveChangesAsync();

            var historyMutationService = new Mock<IQuestionnaireHistoryMutationService>();
            var service = new AnonymousQuestionnaireStateService(dbContext, historyMutationService.Object);

            var result = await service.SaveStateAsync(questionnaireId, true, "Questionnaire title", responsibleId, "designer-user");

            result.IsActive.Should().BeTrue();
            historyMutationService.Verify(x => x.StageQuestionnaireChangeItemAsync(
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

        [Test]
        public async Task when_missing_state_is_requested_disabled_should_not_append_history()
        {
            var questionnaireId = Guid.NewGuid();
            var responsibleId = Guid.NewGuid();
            var dbContext = Create.InMemoryDbContext();
            var historyMutationService = new Mock<IQuestionnaireHistoryMutationService>();
            var service = new AnonymousQuestionnaireStateService(dbContext, historyMutationService.Object);

            var result = await service.SaveStateAsync(questionnaireId, false, "Questionnaire title", responsibleId, "designer-user");

            result.IsActive.Should().BeFalse();
            dbContext.AnonymousQuestionnaires.Single(x => x.QuestionnaireId == questionnaireId).IsActive.Should().BeFalse();
            historyMutationService.Verify(x => x.StageQuestionnaireChangeItemAsync(
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
