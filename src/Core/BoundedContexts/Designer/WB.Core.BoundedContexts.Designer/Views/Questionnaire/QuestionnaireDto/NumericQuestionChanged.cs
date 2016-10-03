using System;
using System.Collections.Generic;
using Main.Core.Entities.SubEntities;
using WB.Core.SharedKernels.QuestionnaireEntities;

namespace WB.Core.BoundedContexts.Designer.Views.Questionnaire.QuestionnaireDto
{
    public class NumericQuestionChanged : AbstractNumericQuestionData
    {
        public NumericQuestionChanged(Guid responsibleId, string conditionExpression, bool hideIfDisabled, bool featured, string instructions, QuestionProperties properties, 
            bool capital, Guid publicKey, 
            string questionText, QuestionScope questionScope, string stataExportCaption, 
            string variableLabel, string validationExpression, string validationMessage, 
            bool? isInteger, int? countOfDecimalPlaces, List<ValidationCondition> validationConditions) : 
            base(responsibleId, conditionExpression, hideIfDisabled, featured, instructions, properties, capital, publicKey, questionText, 
                questionScope, stataExportCaption, variableLabel, validationExpression, validationMessage, isInteger, countOfDecimalPlaces, validationConditions)
        {
        }
    }
}
