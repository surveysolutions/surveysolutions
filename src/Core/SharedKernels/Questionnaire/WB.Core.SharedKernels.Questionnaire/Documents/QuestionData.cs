using System;
using System.Collections.Generic;
using Main.Core.Entities.SubEntities;
using WB.Core.SharedKernels.QuestionnaireEntities;

namespace Main.Core.Entities
{
    public class QuestionData
    {
        public QuestionData(
            Guid publicKey,
            QuestionType questionType,
            QuestionScope questionScope,
            string questionText,
            string stataExportCaption,
            string variableLabel,
            string conditionExpression,
            bool hideIfDisabled,
            Order? answerOrder,
            bool featured,
            bool capital,
            string instructions,
            string mask,
            Answer[] answers,
            Guid? linkedToQuestionId,
            Guid? linkedToRosterId,
            bool? isInteger,
            int? countOfDecimalPlaces,
            bool? areAnswersOrdered,
            int? maxAllowedAnswers,
            int? maxAnswerCount,
            bool? isFilteredCombobox,
            Guid? cascadeFromQuestionId,
            bool? yesNoView,
            IList<ValidationCondition> validationConditions,
            string linkedFilterExpression)
        {
            this.PublicKey = publicKey;
            this.QuestionType = questionType;
            this.QuestionScope = questionScope;
            this.QuestionText = questionText;
            this.StataExportCaption = stataExportCaption;
            this.VariableLabel = variableLabel;
            this.ConditionExpression = conditionExpression;
            this.HideIfDisabled = hideIfDisabled;
            this.AnswerOrder = answerOrder;
            this.Featured = featured;
            this.Capital = capital;
            this.Instructions = instructions;
            this.Answers = answers;
            this.LinkedToQuestionId = linkedToQuestionId;
            this.LinkedToRosterId = linkedToRosterId;
            this.LinkedFilterExpression = linkedFilterExpression;
            this.IsInteger = isInteger;
            this.CountOfDecimalPlaces = countOfDecimalPlaces;

            this.AreAnswersOrdered = areAnswersOrdered;
            this.MaxAllowedAnswers = maxAllowedAnswers;
            this.Mask = mask;
            this.MaxAnswerCount = maxAnswerCount;
            this.IsFilteredCombobox = isFilteredCombobox;
            this.CascadeFromQuestionId = cascadeFromQuestionId;
            this.YesNoView = yesNoView;
            this.ValidationConditions = validationConditions ?? new List<ValidationCondition>();
        }

        public readonly Guid PublicKey;
        public readonly QuestionType QuestionType;
        public readonly QuestionScope QuestionScope;
        public readonly string QuestionText;
        public readonly string StataExportCaption;
        public readonly string VariableLabel;
        public readonly string ConditionExpression;
        public readonly bool HideIfDisabled;
        public readonly string ValidationExpression;
        public readonly string ValidationMessage;
        public readonly Order? AnswerOrder;
        public readonly bool Featured;
        public readonly bool Capital;
        public readonly string Instructions;
        public readonly string Mask;
        public readonly Answer[] Answers;
        public readonly Guid? LinkedToQuestionId;
        public readonly Guid? LinkedToRosterId;
        public readonly string LinkedFilterExpression;
        public readonly bool? IsInteger;
        public readonly int? CountOfDecimalPlaces;

        public readonly bool? AreAnswersOrdered;
        public readonly int? MaxAllowedAnswers;

        public readonly int? MaxAnswerCount;
        public readonly bool? IsFilteredCombobox;
        public readonly Guid? CascadeFromQuestionId;
        public readonly bool? YesNoView;

        public IList<ValidationCondition> ValidationConditions { get; set; } 
    }
}