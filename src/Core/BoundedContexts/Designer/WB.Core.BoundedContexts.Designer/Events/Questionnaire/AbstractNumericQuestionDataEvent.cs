using System;
using System.Collections.Generic;
using Main.Core.Entities.SubEntities;
using WB.Core.SharedKernels.QuestionnaireEntities;

namespace WB.Core.BoundedContexts.Designer.Events.Questionnaire
{
    public class AbstractNumericQuestionDataEvent : AbstractQuestionDataEvent
    {
        public AbstractNumericQuestionDataEvent(Guid responsibleId, string conditionExpression, bool hideIfDisabled, bool featured, string instructions, bool capital, Guid publicKey, 
            string questionText, QuestionScope questionScope, string stataExportCaption, string variableLabel, string validationExpression, string validationMessage, 
            bool? isInteger, int? countOfDecimalPlaces, IList<ValidationCondition> validationConditions) : base(responsibleId, conditionExpression, bool hideIfDisabled, featured, instructions, capital, publicKey, questionText, questionScope, 
                stataExportCaption, variableLabel, validationExpression, validationMessage, validationConditions)
        {
            this.IsInteger = isInteger;
            this.CountOfDecimalPlaces = countOfDecimalPlaces;
        }

        public bool? IsInteger { get; private set; }
        public int? CountOfDecimalPlaces { get; private set; }
    }
}
