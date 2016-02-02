using System;
using System.Collections.Generic;
using System.Linq;
using Main.Core.Entities.SubEntities;
using WB.Core.BoundedContexts.Designer.Commands.Questionnaire.Base;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.Edit.QuestionInfo;

namespace WB.Core.BoundedContexts.Designer.Commands.Questionnaire.Question
{
    [Serializable]
    public class UpdateSingleOptionQuestionCommand : UpdateValidatableQuestionCommand
    {
        public UpdateSingleOptionQuestionCommand(
            Guid questionnaireId,
            Guid questionId,
            string title,
            string variableName, string variableLabel,
            string enablementCondition,
            string instructions,
            Guid responsibleId,
            string validationExpression,
            string validationMessage,
            QuestionScope scope,
            bool isPreFilled,
            Option[] options,
            Guid? linkedToQuestionId,
            bool isFilteredCombobox,
            Guid? cascadeFromQuestionId,
            List<ValidationCondition> validationConditions)
            : base(
                responsibleId: responsibleId, questionnaireId: questionnaireId, questionId: questionId, title: title,
                variableName: variableName, enablementCondition: enablementCondition, 
                instructions: instructions, variableLabel: variableLabel,
                validationConditions: validationConditions)
        {
            this.IsPreFilled = isPreFilled;
            this.Scope = scope;
            options?.ToList()
                .ForEach(x => x.Title = CommandUtils.SanitizeHtml(x.Title, removeAllTags: true));
            this.Options = options;
            this.LinkedToQuestionId = linkedToQuestionId;
            this.IsFilteredCombobox = isFilteredCombobox;
            this.CascadeFromQuestionId = cascadeFromQuestionId;
        }

        public bool IsFilteredCombobox { get; set; }

        public Guid? CascadeFromQuestionId { get; set; }

        public QuestionScope Scope { get; set; }

        public bool IsPreFilled { get; set; }

        public Guid? LinkedToQuestionId { get; set; }

        public Option[] Options { get; set; }
    }
}