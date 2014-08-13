using System;
using Main.Core.Entities.SubEntities;

namespace WB.Core.BoundedContexts.Designer.Commands.Questionnaire.Base
{
    public abstract class AbstractQuestionCommand : QuestionCommand
    {
        protected AbstractQuestionCommand(Guid questionnaireId, Guid questionId,
            string title, string variableName, string mask, bool isMandatory, bool isPreFilled,
            QuestionScope scope, string enablementCondition, string validationExpression, 
            string validationMessage, string instructions,Guid responsibleId, string variableLabel)
            : base(questionnaireId, questionId, responsibleId)
        {
            this.VariableLabel = variableLabel;
            this.Title = CommandUtils.SanitizeHtml(title);
            this.VariableName = variableName;
            this.IsMandatory = isMandatory;
            this.IsPreFilled = isPreFilled;
            this.Scope = scope;
            this.EnablementCondition = enablementCondition;
            this.ValidationExpression = validationExpression;
            this.ValidationMessage = validationMessage;
            this.Instructions = instructions;
            this.Mask = mask;
        }
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
