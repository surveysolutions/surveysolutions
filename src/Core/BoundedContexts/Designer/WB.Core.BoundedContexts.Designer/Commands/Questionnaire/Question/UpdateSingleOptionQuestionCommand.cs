using System;
using System.Linq;
using Main.Core.Entities.SubEntities;
using WB.Core.BoundedContexts.Designer.Commands.Questionnaire.Base;

namespace WB.Core.BoundedContexts.Designer.Commands.Questionnaire.Question
{
    [Serializable]
    public class UpdateSingleOptionQuestionCommand : AbstractUpdateQuestionCommand
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
            Guid? linkedToEntityId,
            bool isFilteredCombobox,
            Guid? cascadeFromQuestionId)
            : base(
                responsibleId: responsibleId, questionnaireId: questionnaireId, questionId: questionId, title: title,
                variableName: variableName, enablementCondition: enablementCondition, 
                instructions: instructions, variableLabel: variableLabel)
        {
            this.IsPreFilled = isPreFilled;
            this.Scope = scope;
            this.ValidationMessage = CommandUtils.SanitizeHtml(validationMessage, removeAllTags: true);
            this.ValidationExpression = validationExpression;
            if (options != null)
                options
                    .ToList()
                    .ForEach(x => x.Title = CommandUtils.SanitizeHtml(x.Title, removeAllTags: true));
            this.Options = options;
            this.LinkedToEntityId = linkedToEntityId;
            this.IsFilteredCombobox = isFilteredCombobox;
            this.CascadeFromQuestionId = cascadeFromQuestionId;
        }

        public bool IsFilteredCombobox { get; set; }

        public Guid? CascadeFromQuestionId { get; set; }

        public QuestionScope Scope { get; set; }

        public string ValidationMessage { get; set; }

        public string ValidationExpression { get; set; }

        public bool IsPreFilled { get; set; }

        public Guid? LinkedToEntityId { get; set; }

        public Option[] Options { get; set; }
    }
}