using System;
using WB.Core.SharedKernels.QuestionnaireEntities;

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
            this.EnablementCondition = commonQuestionParameters.EnablementCondition ?? string.Empty;
            this.HideIfDisabled = commonQuestionParameters.HideIfDisabled;
            this.Instructions = CommandUtils.SanitizeHtml(commonQuestionParameters.Instructions);
            this.Properties = new QuestionProperties(commonQuestionParameters.HideInstructions, false)
            {
                OptionsFilterExpression = commonQuestionParameters.OptionsFilterExpression,
                GeometryType = commonQuestionParameters.GeometryType,
                GeometryInputMode = commonQuestionParameters.GeometryInputMode
            };
        }

        public string? Title { get; private set; }
        public string? VariableName { get; private set; }
        public string VariableLabel { get; private set; }
        public string EnablementCondition { get; set; }
        public bool HideIfDisabled { get; set; }
        public string? Instructions { get; private set; }
        public QuestionProperties Properties { get; set; }
    }
}
