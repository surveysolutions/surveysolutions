using System;
using System.Collections.Generic;
using System.Linq;
using Main.Core.Entities.SubEntities;
using WB.Core.BoundedContexts.Designer.Commands.Questionnaire.Base;
using WB.Core.SharedKernels.QuestionnaireEntities;

namespace WB.Core.BoundedContexts.Designer.Commands.Questionnaire.Question
{
    [Serializable]
    public class UpdateSingleOptionQuestion : UpdateValidatableQuestionCommand
    {
        public UpdateSingleOptionQuestion(
            Guid questionnaireId,
            Guid questionId,
            Guid responsibleId,
            CommonQuestionParameters commonQuestionParameters,
            string validationExpression,
            string validationMessage,
            QuestionScope scope,
            bool isPreFilled,
            Option[] options,
            Guid? linkedToEntityId,
            bool isFilteredCombobox,
            Guid? cascadeFromQuestionId,
            List<ValidationCondition> validationConditions,
            string linkedFilterExpression)
            : base(
                responsibleId: responsibleId, questionnaireId: questionnaireId, questionId: questionId, 
                commonQuestionParameters: commonQuestionParameters,
                validationConditions: validationConditions)
        {
            this.IsPreFilled = isPreFilled;
            this.Scope = scope;
            options?.ToList()
                .ForEach(x => x.Title = CommandUtils.SanitizeHtml(x.Title, removeAllTags: true));
            this.Options = options;
            this.LinkedToEntityId = linkedToEntityId;
            this.LinkedFilterExpression = linkedFilterExpression;
            this.IsFilteredCombobox = isFilteredCombobox;
            this.CascadeFromQuestionId = cascadeFromQuestionId;
        }

        public bool IsFilteredCombobox { get; set; }

        public Guid? CascadeFromQuestionId { get; set; }

        public QuestionScope Scope { get; set; }

        public bool IsPreFilled { get; set; }

        public Guid? LinkedToEntityId { get; set; }

        public string LinkedFilterExpression { get; set; }

        public Option[] Options { get; set; }
    }
}