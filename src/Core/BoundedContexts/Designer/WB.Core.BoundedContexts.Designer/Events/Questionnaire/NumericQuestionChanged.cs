﻿using System;
using Main.Core.Entities.SubEntities;

namespace WB.Core.BoundedContexts.Designer.Events.Questionnaire
{
    public class NumericQuestionChanged : AbstractNumericQuestionDataEvent
    {
        public NumericQuestionChanged(Guid responsibleId, string conditionExpression, bool featured, string instructions, bool capital, Guid publicKey, 
            string questionText, QuestionScope questionScope, string stataExportCaption, string variableLabel, string validationExpression, string validationMessage, 
            bool? isInteger, int? countOfDecimalPlaces) : base(responsibleId, conditionExpression, featured, instructions, capital, publicKey, questionText, 
                questionScope, stataExportCaption, variableLabel, validationExpression, validationMessage, isInteger, countOfDecimalPlaces)
        {
        }
    }
}
