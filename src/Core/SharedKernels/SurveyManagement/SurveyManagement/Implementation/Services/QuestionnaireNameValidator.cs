using System;
using System.Linq;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernels.DataCollection.Commands.Questionnaire;
using WB.Core.SharedKernels.DataCollection.Exceptions;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates;
using WB.Core.SharedKernels.DataCollection.Views.Questionnaire;
using WB.Core.SharedKernels.SurveyManagement.Views.Questionnaire;

namespace WB.Core.SharedKernels.SurveyManagement.Implementation.Services
{
    internal class QuestionnaireNameValidator : ICommandValidator<Questionnaire, ImportFromDesigner>
    {
        private readonly IQueryableReadSideRepositoryReader<QuestionnaireBrowseItem> questionnaireBrowseItemStorage;

        public QuestionnaireNameValidator(IQueryableReadSideRepositoryReader<QuestionnaireBrowseItem> questionnaireBrowseItemStorage)
        {
            this.questionnaireBrowseItemStorage = questionnaireBrowseItemStorage;
        }

        public void Validate(Questionnaire aggregate, ImportFromDesigner command)
        {
            if (DoesOtherQuestionnaireWithSameTitleExist(command.QuestionnaireId, command.Source.Title))
                throw new QuestionnaireException(
                    $"You have already imported other questionnaire with title '{command.Source.Title}' from Designer. If you still want to import this particular one, please rename it using Designer.");
        }

        private bool DoesOtherQuestionnaireWithSameTitleExist(Guid questionnaireId, string questionnaireTitle)
        {
            var questionairesWithSameTitle = this.questionnaireBrowseItemStorage.Query(_ => _
                .Where(questionnaire => questionnaire.Title.ToLower() == questionnaireTitle.ToLower())
                .ToList());

            var otherQuestionnairesWithSameTitle = questionairesWithSameTitle
                .Where(questionnaire => questionnaire.QuestionnaireId != questionnaireId);

            return otherQuestionnairesWithSameTitle.Any();
        }
    }
}