using System;
using System.Collections.Generic;
using Main.Core.Entities.SubEntities;

namespace Main.Core.Entities
{
    public class QuestionData
    {
        public QuestionData(){}
        public QuestionData(
            Guid publicKey,
            QuestionType questionType,
            QuestionScope questionScope,
            string questionText,
            string stataExportCaption,
            string conditionExpression,
            string validationExpression,
            string validationMessage,
            Order answerOrder,
            bool featured,
            bool mandatory,
            bool capital,
            string instructions,
            List<Guid> triggers,
            int maxValue,
            Answer[] answers,
            Guid? linkedToQuestionId,
            bool? isInteger,
            int? countOfDecimalPlaces,
            bool? isAnswersOrdered,
            int? maxAllowedAnswers
            )
        {
            this.publicKey = publicKey;
            this.questionType = questionType;
            this.questionScope = questionScope;
            this.questionText = questionText;
            this.stataExportCaption = stataExportCaption;
            this.conditionExpression = conditionExpression;
            this.validationExpression = validationExpression;
            this.validationMessage = validationMessage;
            this.answerOrder = answerOrder;
            this.featured = featured;
            this.mandatory = mandatory;
            this.capital = capital;
            this.instructions = instructions;
            this.triggers = triggers;
            this.maxValue = maxValue;
            this.answers = answers;
            this.linkedToQuestionId = linkedToQuestionId;
            this.isInteger = isInteger;
            this.countOfDecimalPlaces = countOfDecimalPlaces;

            this.isAnswersOrdered = isAnswersOrdered;
            this.maxAllowedAnswers = maxAllowedAnswers;
        }
        public Guid publicKey { get; set; }
        public QuestionType questionType { get; set; }
        public QuestionScope questionScope { get; set; }
        public string questionText { get; set; }
        public string stataExportCaption { get; set; }
        public string conditionExpression { get; set; }
        public string validationExpression { get; set; }
        public string validationMessage { get; set; }
        public Order answerOrder { get; set; }
        public bool featured { get; set; }
        public bool mandatory { get; set; }
        public bool capital { get; set; }
        public string instructions { get; set; }
        public List<Guid> triggers { get; set; }
        public int maxValue { get; set; }
        public Answer[] answers { get; set; }
        public Guid? linkedToQuestionId { get; set; }
        public bool? isInteger { get; set; }
        public int? countOfDecimalPlaces { get; set; }

        public bool? isAnswersOrdered { get; set; }
        public int? maxAllowedAnswers { get; set; }
    }
}