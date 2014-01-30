using System;

namespace WB.Core.BoundedContexts.Designer.Commands.Questionnaire.Base
{
    public abstract class AbstractListQuestionCommand : QuestionCommand
    {
        protected AbstractListQuestionCommand(
            Guid responsibleId, 
            Guid questionnaireId, 
            Guid questionId, 
            string title, 
            string variableName, 
            bool isMandatory, 
            string condition, 
            string instructions, 
            int? maxAnswerCount)
            : base(questionnaireId, questionId, responsibleId)
        {
            this.Title = title;
            this.VariableName = variableName;
            this.IsMandatory = isMandatory;
            this.Condition = condition;
            this.Instructions = instructions;
            this.MaxAnswerCount = maxAnswerCount;
        }

        public string Title { get; private set; }

        public string VariableName { get; private set; }

        public bool IsMandatory { get; private set; }

        public string Condition { get; set; }

        public string Instructions { get; private set; }

        public int? MaxAnswerCount { get; private set; }
    }
}
