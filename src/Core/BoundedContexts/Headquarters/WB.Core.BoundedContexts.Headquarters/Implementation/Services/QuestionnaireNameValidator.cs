using System;
using System.Linq;
using WB.Core.BoundedContexts.Headquarters.Commands;
using WB.Core.BoundedContexts.Headquarters.Implementation.Aggregates;
using WB.Core.BoundedContexts.Headquarters.Views.Questionnaire;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.SharedKernels.DataCollection.Exceptions;

namespace WB.Core.BoundedContexts.Headquarters.Implementation.Services
{
    internal class QuestionnaireNameValidator : ICommandValidator<Questionnaire, ImportFromDesigner>
    {
        private readonly IPlainStorageAccessor<QuestionnaireBrowseItem> questionnaireBrowseItemStorage;

        public QuestionnaireNameValidator(IPlainStorageAccessor<QuestionnaireBrowseItem> questionnaireBrowseItemStorage)
        {
            this.questionnaireBrowseItemStorage = questionnaireBrowseItemStorage;
        }

        public void Validate(Questionnaire aggregate, ImportFromDesigner command)
        {
            if (this.DoesOtherQuestionnaireWithSameTitleExist(command.QuestionnaireId, command.Source.Title))
                throw new QuestionnaireException(
                    $"You have already imported other questionnaire with title '{command.Source.Title}' from Designer. If you still want to import this particular one, please rename it using Designer.");
        }

        private bool DoesOtherQuestionnaireWithSameTitleExist(Guid questionnaireId, string questionnaireTitle)
        {
            var questionairesWithSameTitle = this.questionnaireBrowseItemStorage.Query(_ => _
                .Where(questionnaire => !questionnaire.IsDeleted)
                .Where(questionnaire => questionnaire.Title.ToLower() == questionnaireTitle.ToLower())
                .ToList());

            var otherQuestionnairesWithSameTitle = questionairesWithSameTitle
                .Where(questionnaire => questionnaire.QuestionnaireId != questionnaireId);

            return otherQuestionnairesWithSameTitle.Any();
        }
    }
}