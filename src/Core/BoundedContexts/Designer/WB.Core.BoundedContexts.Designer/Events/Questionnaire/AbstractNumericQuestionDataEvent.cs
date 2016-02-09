using System;
using Main.Core.Entities.SubEntities;

namespace WB.Core.BoundedContexts.Designer.Events.Questionnaire
{
    public class AbstractNumericQuestionDataEvent : AbstractQuestionDataEvent
    {
        public AbstractNumericQuestionDataEvent(Guid responsibleId, string conditionExpression, bool hideIfDisabled, bool featured, string instructions, bool capital, Guid publicKey, 
            string questionText, QuestionScope questionScope, string stataExportCaption, string variableLabel, string validationExpression, string validationMessage, 
            bool? isInteger, int? countOfDecimalPlaces) : base(responsibleId, conditionExpression, hideIfDisabled, featured, instructions, capital, publicKey, questionText, questionScope, 
                stataExportCaption, variableLabel, validationExpression, validationMessage)
        {
            this.IsInteger = isInteger;
            this.CountOfDecimalPlaces = countOfDecimalPlaces;
        }

        public bool? IsInteger { get; private set; }
        public int? CountOfDecimalPlaces { get; private set; }
    }
}
