using System;
using Main.Core.Entities.SubEntities;

namespace WB.Core.BoundedContexts.Designer.Commands.Questionnaire.Base
{
    public abstract class AbstractListQuestionCommand : AbstractQuestionCommand
    {
        protected AbstractListQuestionCommand(Guid questionnaireId, Guid questionId, string title, string alias, 
            bool isMandatory, bool isFeatured, QuestionScope scope, string condition, string validationExpression, 
            string validationMessage, string instructions, Guid responsibleId, int? maxAnswerCount)
            : base(questionnaireId, questionId, title, alias, isMandatory, isFeatured, 
                scope, condition, validationExpression, validationMessage, instructions, responsibleId)
        {
            this.MaxAnswerCount = maxAnswerCount;
        }

        public int? MaxAnswerCount { get; private set; }
    }
}
