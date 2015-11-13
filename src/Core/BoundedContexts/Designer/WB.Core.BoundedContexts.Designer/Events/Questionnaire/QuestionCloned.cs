﻿using System;
using Main.Core.Entities.SubEntities;

namespace Main.Core.Events.Questionnaire
{
    public class QuestionCloned : FullQuestionDataEvent
    {
        protected QuestionCloned()
        {
        }

        public QuestionCloned(Guid responsibleId, string conditionExpression, bool featured, string instructions, bool capital, Guid publicKey, string questionText, 
            QuestionScope questionScope, string stataExportCaption, string variableLabel, string validationExpression, string validationMessage, QuestionType questionType, 
            Order? answerOrder, Answer[] answers, Guid? groupPublicKey, Guid? linkedToQuestionId, bool? isInteger, bool? areAnswersOrdered, bool? yesNoView, int? maxAllowedAnswers, 
            string mask, bool? isFilteredCombobox, Guid? cascadeFromQuestionId, Guid? sourceQuestionnaireId, Guid sourceQuestionId, int targetIndex, int? maxAnswerCount, int? countOfDecimalPlaces) 
            : base(responsibleId, conditionExpression, featured, instructions, capital, publicKey, questionText, questionScope, stataExportCaption, variableLabel, validationExpression, 
                  validationMessage, questionType, answerOrder, answers, groupPublicKey, linkedToQuestionId, isInteger, areAnswersOrdered, yesNoView, maxAllowedAnswers, mask, isFilteredCombobox,
                  cascadeFromQuestionId)
        {
            this.SourceQuestionnaireId = sourceQuestionnaireId;
            this.SourceQuestionId = sourceQuestionId;
            this.TargetIndex = targetIndex;
            this.MaxAnswerCount = maxAnswerCount;
            this.CountOfDecimalPlaces = countOfDecimalPlaces;
        }

        public Guid? SourceQuestionnaireId { get; private set; }
        public Guid SourceQuestionId { get; private set; }
        public int TargetIndex { get; private set; }

        /// <summary>
        /// Gets or sets count of allowed answers for list question
        /// </summary>
        public int? MaxAnswerCount { get; private set; }

        public int? CountOfDecimalPlaces { get; private set; }
    }
}