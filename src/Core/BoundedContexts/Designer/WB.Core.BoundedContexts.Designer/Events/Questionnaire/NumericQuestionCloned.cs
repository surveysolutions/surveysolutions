using System;
using System.Collections.Generic;
using Main.Core.Entities.SubEntities;
using WB.Core.SharedKernels.QuestionnaireEntities;

namespace WB.Core.BoundedContexts.Designer.Events.Questionnaire
{
    public class NumericQuestionCloned : AbstractNumericQuestionDataEvent
    {
        public NumericQuestionCloned(Guid responsibleId, string conditionExpression, bool featured, string instructions, bool capital, Guid publicKey, string questionText, 
            QuestionScope questionScope, string stataExportCaption, string variableLabel, string validationExpression, string validationMessage, bool? isInteger, int? countOfDecimalPlaces, 
            Guid? sourceQuestionnaireId, Guid sourceQuestionId, Guid groupPublicKey, int targetIndex) : base(responsibleId, conditionExpression, featured, instructions, capital, 
                publicKey, questionText, questionScope, stataExportCaption, variableLabel, validationExpression, validationMessage, isInteger, countOfDecimalPlaces, new List<ValidationCondition>()) // todo : KP-6698
        {
            this.SourceQuestionnaireId = sourceQuestionnaireId;
            this.SourceQuestionId = sourceQuestionId;
            this.GroupPublicKey = groupPublicKey;
            this.TargetIndex = targetIndex;
        }

        public Guid? SourceQuestionnaireId { get; private set; }
        public Guid SourceQuestionId { get; private set; }
        public Guid GroupPublicKey { get; private set; }
        public int TargetIndex { get; private set; }
    }
}
