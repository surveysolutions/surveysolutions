using System;
using Main.Core.Entities.SubEntities;

namespace Main.Core.Events.Questionnaire
{
    public class QuestionChanged : FullQuestionDataEvent
    {
        protected QuestionChanged() { }

        public QuestionChanged(Guid responsibleId, string conditionExpression, bool hideIfDisabled, bool featured, string instructions, bool capital, Guid publicKey, string questionText, 
            QuestionScope questionScope, string stataExportCaption, string variableLabel, string validationExpression, string validationMessage, QuestionType questionType, 
            Order? answerOrder, Answer[] answers, Guid? groupPublicKey, Guid? linkedToQuestionId, Guid? linkedToRosterId, bool? isInteger, bool? areAnswersOrdered, bool? yesNoView, int? maxAllowedAnswers, 
            string mask, bool? isFilteredCombobox, Guid? cascadeFromQuestionId, Guid targetGroupKey) : base(responsibleId, conditionExpression, hideIfDisabled, featured, instructions, capital, publicKey, 
                questionText, questionScope, stataExportCaption, variableLabel, validationExpression, validationMessage, questionType, answerOrder, answers, groupPublicKey, 
                linkedToQuestionId, linkedToRosterId, isInteger, areAnswersOrdered, yesNoView, maxAllowedAnswers, mask, isFilteredCombobox, cascadeFromQuestionId)
        {
            this.TargetGroupKey = targetGroupKey;
        }

        public Guid TargetGroupKey { get; private set; }
    }
}