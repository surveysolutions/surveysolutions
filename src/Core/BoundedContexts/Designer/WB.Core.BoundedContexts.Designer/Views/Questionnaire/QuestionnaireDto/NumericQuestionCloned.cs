using System;
using System.Collections.Generic;
using Main.Core.Entities.SubEntities;
using WB.Core.SharedKernels.QuestionnaireEntities;

namespace WB.Core.BoundedContexts.Designer.Views.Questionnaire.QuestionnaireDto
{
    public class NumericQuestionCloned : AbstractNumericQuestionData
    {
        public NumericQuestionCloned(Guid responsibleId, string conditionExpression, bool hideIfDisabled, bool featured, string instructions, QuestionProperties properties, bool capital, Guid publicKey, string questionText, 
            QuestionScope questionScope, string stataExportCaption, string variableLabel, string validationExpression, string validationMessage, bool? isInteger, int? countOfDecimalPlaces, 
            Guid? sourceQuestionnaireId, Guid sourceQuestionId, Guid groupPublicKey, int targetIndex, IList<ValidationCondition> validationConditions) 
            : base(responsibleId, conditionExpression, hideIfDisabled, featured, instructions, properties, capital, 
                publicKey, questionText, questionScope, stataExportCaption, variableLabel, 
                validationExpression, validationMessage, isInteger, countOfDecimalPlaces,
                validationConditions) 
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
