using System;

namespace WB.Core.BoundedContexts.Designer.Commands.Questionnaire.Base
{
    public abstract class AbstractUpdateQuestionCommand : QuestionCommand
    {
        protected AbstractUpdateQuestionCommand(Guid responsibleId,
            Guid questionnaireId,
            Guid questionId, string title, string variableName, string variableLabel, bool isMandatory, string enablementCondition, string instructions)
            : base(questionnaireId, questionId, responsibleId)
        {
            this.VariableLabel = CommandUtils.SanitizeHtml(variableLabel, removeAllTags: true);
            this.Title = CommandUtils.SanitizeHtml(title);
            this.VariableName = CommandUtils.SanitizeHtml(variableName, removeAllTags: true);
            this.IsMandatory = isMandatory;
            this.EnablementCondition = enablementCondition;
            this.Instructions = instructions;
        }

        public string Title { get; private set; }
        public string VariableName { get; private set; }
        public string VariableLabel { get; private set; }
        public bool IsMandatory { get; private set; }
        public string EnablementCondition { get; set; }
        public string Instructions { get; private set; }
    }
}