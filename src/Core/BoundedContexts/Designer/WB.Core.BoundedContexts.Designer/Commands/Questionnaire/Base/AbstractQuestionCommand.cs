using System;
using Main.Core.Entities.SubEntities;

namespace WB.Core.BoundedContexts.Designer.Commands.Questionnaire.Base
{
    public abstract class AbstractQuestionCommand : QuestionCommand
    {
        protected AbstractQuestionCommand(Guid questionnaireId, Guid questionId,
            string title, string variableName, string mask, bool isMandatory, bool isPreFilled,
            QuestionScope scope, string enablementCondition, string validationExpression,
            string validationMessage, string instructions, Guid responsibleId, string variableLabel, bool? isFilteredCombobox)
            : base(questionnaireId, questionId, responsibleId)
        {
            this.VariableLabel = CommandUtils.SanitizeHtml(variableLabel, removeAllTags: true);
            this.Title = CommandUtils.SanitizeHtml(title);
            this.VariableName = CommandUtils.SanitizeHtml(variableName, removeAllTags: true);
            this.IsMandatory = isMandatory;
            this.IsPreFilled = isPreFilled;
            this.Scope = scope;
            this.EnablementCondition = enablementCondition;
            this.ValidationExpression = validationExpression;
            this.ValidationMessage = CommandUtils.SanitizeHtml(validationMessage, removeAllTags: true);
            this.Instructions = CommandUtils.SanitizeHtml(instructions, removeAllTags: true);
            this.Mask = mask;
            this.IsFilteredCombobox = isFilteredCombobox;
        }

        public bool? IsFilteredCombobox { get; set; }

        public string Title { get; private set; }

        public string VariableName { get; private set; }

        public string VariableLabel { get; private set; }

        public bool IsMandatory { get; private set; }

        public bool IsPreFilled { get; private set; }

        public QuestionScope Scope { get; private set; }

        public string EnablementCondition { get; set; }

        public string ValidationExpression { get; set; }

        public string ValidationMessage { get; private set; }

        public string Instructions { get; private set; }

        public string Mask { get; private set; }
    }
}
