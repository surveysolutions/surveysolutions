using System;
using System.Collections.Generic;
using Main.Core.Entities.SubEntities;
using WB.Core.SharedKernels.QuestionnaireEntities;

namespace WB.Core.BoundedContexts.Designer.Views.Questionnaire.QuestionnaireDto
{
    public class AbstractNumericQuestionData : AbstractQuestionData
    {
        public AbstractNumericQuestionData(Guid responsibleId, string conditionExpression, bool hideIfDisabled, bool featured, string instructions, QuestionProperties properties, 
            bool capital, Guid publicKey, 
            string questionText, QuestionScope questionScope, 
            string stataExportCaption, string variableLabel, string validationExpression, string validationMessage, 
            bool? isInteger, int? countOfDecimalPlaces, IList<ValidationCondition> validationConditions) : 
            base(responsibleId, conditionExpression, hideIfDisabled, featured, instructions, properties, capital, publicKey, questionText, questionScope, 
                stataExportCaption, variableLabel, validationExpression, validationMessage, validationConditions)
        {
            this.IsInteger = isInteger;
            this.CountOfDecimalPlaces = countOfDecimalPlaces;
        }

        public bool? IsInteger { get; private set; }
        public int? CountOfDecimalPlaces { get; private set; }
    }
}
