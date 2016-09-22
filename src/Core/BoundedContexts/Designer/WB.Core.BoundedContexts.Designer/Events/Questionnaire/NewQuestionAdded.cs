using System;
using System.Collections.Generic;
using Main.Core.Entities.SubEntities;
using WB.Core.BoundedContexts.Designer.Events.Questionnaire;
using WB.Core.SharedKernels.QuestionnaireEntities;

namespace Main.Core.Events.Questionnaire
{
    public class NewQuestionAdded : FullQuestionDataEvent
    {
        protected NewQuestionAdded()
        {
        }

        public NewQuestionAdded(Guid responsibleId, string conditionExpression, bool hideIfDisabled, bool featured, string instructions, QuestionProperties properties, bool capital, Guid publicKey, string questionText, 
            QuestionScope questionScope, string stataExportCaption, string variableLabel, string validationExpression, string validationMessage, QuestionType questionType, 
            Order? answerOrder, Answer[] answers, Guid? groupPublicKey, Guid? linkedToQuestionId, bool? isInteger, bool? areAnswersOrdered, bool? yesNoView, int? maxAllowedAnswers, 
            string mask, bool? isFilteredCombobox, Guid? cascadeFromQuestionId, IList<ValidationCondition> validationConditions) : 
            base(responsibleId, conditionExpression, hideIfDisabled, featured, instructions, properties, capital, publicKey, questionText, 
                questionScope, stataExportCaption, variableLabel, validationExpression, validationMessage, questionType, answerOrder, answers, groupPublicKey, linkedToQuestionId, null, isInteger, 
                areAnswersOrdered, yesNoView, maxAllowedAnswers, mask, isFilteredCombobox, cascadeFromQuestionId, validationConditions, null, false)
        {
        }
    }
}