using System;
using System.Collections.Generic;
using Main.Core.Entities.SubEntities;
using WB.Core.SharedKernels.QuestionnaireEntities;

namespace WB.Core.BoundedContexts.Designer.Views.Questionnaire.QuestionnaireDto
{
    public class QuestionChanged : FullQuestionData
    {
        protected QuestionChanged() { }

        public QuestionChanged(
            Guid responsibleId, 
            string conditionExpression, 
            bool hideIfDisabled, 
            bool featured, 
            string instructions,
            QuestionProperties properties,
            bool capital, 
            Guid publicKey, 
            string questionText, 
            QuestionScope questionScope, 
            string stataExportCaption, 
            string variableLabel, 
            string validationExpression, 
            string validationMessage, 
            QuestionType questionType, 
            Order? answerOrder, 
            Answer[] answers, 
            Guid? groupPublicKey, 
            Guid? linkedToQuestionId, 
            Guid? linkedToRosterId, 
            bool? isInteger, 
            bool? areAnswersOrdered, 
            bool? yesNoView, 
            int? maxAllowedAnswers, 
            string mask, 
            bool? isFilteredCombobox, 
            Guid? cascadeFromQuestionId, 
            Guid targetGroupKey, 
            IList<ValidationCondition> validationConditions,
            string linkedFilterExpression,
            bool isTimestamp) : base(responsibleId, conditionExpression, hideIfDisabled, featured, instructions, properties, capital, publicKey, 
                questionText, questionScope, stataExportCaption, variableLabel, validationExpression, validationMessage, questionType, answerOrder, answers, groupPublicKey, 
                linkedToQuestionId, linkedToRosterId, isInteger, areAnswersOrdered, yesNoView, maxAllowedAnswers, mask, isFilteredCombobox, cascadeFromQuestionId, validationConditions, linkedFilterExpression, isTimestamp)
        {
            this.TargetGroupKey = targetGroupKey;
        }

        public Guid TargetGroupKey { get; private set; }
    }
}