using System;
using System.Collections.Generic;
using System.Linq;
using Main.Core.Entities.SubEntities;
using WB.Core.BoundedContexts.Designer.Commands.Questionnaire.Base;
using WB.Core.SharedKernels.QuestionnaireEntities;

namespace WB.Core.BoundedContexts.Designer.Commands.Questionnaire.Question
{
    [Serializable]
    public class UpdateMultiOptionQuestion : UpdateValidatableQuestionCommand
    {
        public UpdateMultiOptionQuestion(
            Guid questionnaireId,
            Guid questionId,
            Guid responsibleId,
            CommonQuestionParameters commonQuestionParameters,
            string validationExpression,
            string validationMessage,
            QuestionScope scope,
            Option[] options,
            Guid? linkedToEntityId,
            bool? areAnswersOrdered,
            int? maxAllowedAnswers,
            bool? yesNoView,
            List<ValidationCondition> validationConditions,
            string linkedFilterExpression,
            bool isFilteredCombobox,
            Guid? categoriesId)
            : base(
                responsibleId: responsibleId, questionnaireId: questionnaireId, questionId: questionId, 
                commonQuestionParameters: commonQuestionParameters,
                validationConditions: validationConditions)
        {
            this.Scope = scope;
            options?.ToList()
                .ForEach(x => x.Title = CommandUtils.SanitizeHtml(x.Title, removeAllTags: true));
            this.Options = options;
            this.LinkedToEntityId = linkedToEntityId;
            this.LinkedFilterExpression = linkedFilterExpression;
            this.AreAnswersOrdered = areAnswersOrdered;
            this.MaxAllowedAnswers = maxAllowedAnswers;
            this.YesNoView = yesNoView;
            this.IsFilteredCombobox = isFilteredCombobox;
            this.CategoriesId = categoriesId;
        }

        public bool IsFilteredCombobox { get; set; }

        public QuestionScope Scope { get; set; }

        public bool? AreAnswersOrdered { get; set; }

        public Guid? LinkedToEntityId { get; set; }

        public string LinkedFilterExpression { get; set; }

        public int? MaxAllowedAnswers { get; set; }

        public bool? YesNoView { get; set; }

        public Option[]? Options { get; set; }
        public Guid? CategoriesId { get; set; }
    }
}
