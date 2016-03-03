using System;

namespace WB.Core.BoundedContexts.Designer.Commands.Questionnaire.Base
{
    public abstract class AbstractUpdateQuestionCommand : QuestionCommand
    {
        protected AbstractUpdateQuestionCommand(Guid responsibleId,
            Guid questionnaireId,
            Guid questionId,
            CommonQuestionParameters commonQuestionParameters)
            : base(questionnaireId, questionId, responsibleId)
        {
            this.VariableLabel = CommandUtils.SanitizeHtml(commonQuestionParameters.VariableLabel, removeAllTags: true);
            this.Title = CommandUtils.SanitizeHtml(commonQuestionParameters.Title);
            this.VariableName = CommandUtils.SanitizeHtml(commonQuestionParameters.VariableName, removeAllTags: true);
            this.EnablementCondition = commonQuestionParameters.EnablementCondition;
            this.HideIfDisabled = commonQuestionParameters.HideIfDisabled;
            this.Instructions = CommandUtils.SanitizeHtml(commonQuestionParameters.Instructions, removeAllTags: true);
        }

        public string Title { get; private set; }
        public string VariableName { get; private set; }
        public string VariableLabel { get; private set; }
        public string EnablementCondition { get; set; }
        public bool HideIfDisabled { get; set; }
        public string Instructions { get; private set; }
    }
}