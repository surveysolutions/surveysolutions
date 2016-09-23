using System;
using Main.Core.Entities.SubEntities;

namespace WB.Core.BoundedContexts.Designer.Events.Questionnaire
{
    public class NumericQuestionAdded : AbstractNumericQuestionDataEvent
    {
        public NumericQuestionAdded(Guid responsibleId, string conditionExpression, bool hideIfDisabled, bool featured, string instructions, bool capital, Guid publicKey, string questionText, 
            QuestionScope questionScope, string stataExportCaption, string variableLabel, string validationExpression, string validationMessage, bool? isInteger, int? countOfDecimalPlaces, 
            Guid groupPublicKey) : base(responsibleId, conditionExpression, hideIfDisabled, featured, instructions, capital, publicKey, questionText, questionScope, stataExportCaption, variableLabel, 
                validationExpression, validationMessage, isInteger, countOfDecimalPlaces)
        {
            this.GroupPublicKey = groupPublicKey;
        }

        public Guid GroupPublicKey { get; private set; }
    }
}
