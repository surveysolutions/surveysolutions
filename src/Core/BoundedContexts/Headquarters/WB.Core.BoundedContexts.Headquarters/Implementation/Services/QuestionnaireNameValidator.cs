using System;
using System.Linq;
using WB.Core.BoundedContexts.Headquarters.Commands;
using WB.Core.BoundedContexts.Headquarters.Implementation.Aggregates;
using WB.Core.BoundedContexts.Headquarters.Resources;
using WB.Core.BoundedContexts.Headquarters.Views.Questionnaire;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.SharedKernels.DataCollection.Exceptions;

namespace WB.Core.BoundedContexts.Headquarters.Implementation.Services
{
    internal class QuestionnaireImportValidator : ICommandValidator<Questionnaire, ImportFromDesigner>
    {
        private readonly IPlainStorageAccessor<QuestionnaireBrowseItem> questionnaireBrowseItemStorage;

        public QuestionnaireImportValidator(IPlainStorageAccessor<QuestionnaireBrowseItem> questionnaireBrowseItemStorage)
        {
            this.questionnaireBrowseItemStorage = questionnaireBrowseItemStorage;
        }

        public void Validate(Questionnaire aggregate, ImportFromDesigner command)
        {
            if (this.DoesOtherQuestionnaireWithSameTitleExist(command.QuestionnaireId, command.Source.Title))
                throw new QuestionnaireException(string.Format(CommandValidatorsMessages.QuestionnaireNameUniqueFormat, command.Source.Title));

            if (!string.IsNullOrWhiteSpace(command.Source.VariableName))
            {
                if (this.DoesOtherQuestionnaireHasSameQuestionnaireVariable(command.QuestionnaireId,
                    command.Source.VariableName))
                    throw new QuestionnaireException(string.Format(CommandValidatorsMessages.QuestionnaireVariableUniqueFormat, command.Source.VariableName));
            }
        }

        private bool DoesOtherQuestionnaireHasSameQuestionnaireVariable(Guid questionnaireId, string variableName)
        {
            var questionairesWithSameVariable = this.questionnaireBrowseItemStorage.Query(_ => _
                .Where(questionnaire => !questionnaire.IsDeleted)
                .Where(questionnaire => questionnaire.Variable.ToLower() == variableName.ToLower())
                .ToList());

            var otherQuestionnairesWithSameTitle = questionairesWithSameVariable
                .Where(questionnaire => questionnaire.QuestionnaireId != questionnaireId);

            return otherQuestionnairesWithSameTitle.Any();
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
