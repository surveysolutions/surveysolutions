using System;
using System.Collections.Generic;
using Main.Core.Entities.SubEntities;
using WB.Core.BoundedContexts.Designer.Events.Questionnaire;

namespace Main.Core.Events.Questionnaire
{
    public class FullQuestionDataEvent : AbstractQuestionDataEvent
    {
        protected FullQuestionDataEvent()
        {
        }

        public FullQuestionDataEvent(Guid responsibleId, string conditionExpression, bool featured, string instructions, bool capital, Guid publicKey, string questionText, 
            QuestionScope questionScope, string stataExportCaption, string variableLabel, string validationExpression, string validationMessage, QuestionType questionType, 
            Order? answerOrder, Answer[] answers, Guid? groupPublicKey, Guid? linkedToQuestionId, Guid? linkedToRosterId, bool? isInteger, bool? areAnswersOrdered, bool? yesNoView, int? maxAllowedAnswers, 
            string mask, bool? isFilteredCombobox, Guid? cascadeFromQuestionId) 
            : base(responsibleId, conditionExpression, featured, instructions, capital, publicKey, questionText, questionScope, stataExportCaption, variableLabel, validationExpression, 
                  validationMessage)
        {
            this.QuestionType = questionType;
            this.AnswerOrder = answerOrder;
            this.Answers = answers;
            this.GroupPublicKey = groupPublicKey;
            this.LinkedToQuestionId = linkedToQuestionId;
            this.LinkedToRosterId = linkedToRosterId;
            this.IsInteger = isInteger;
            this.AreAnswersOrdered = areAnswersOrdered;
            this.YesNoView = yesNoView;
            this.MaxAllowedAnswers = maxAllowedAnswers;
            this.Mask = mask;
            this.IsFilteredCombobox = isFilteredCombobox;
            this.CascadeFromQuestionId = cascadeFromQuestionId;
        }

        public QuestionType QuestionType { get; private set; }
        public Order? AnswerOrder { get; private set; }
        public Answer[] Answers { get; private set; }
        public Guid? GroupPublicKey { get; private set; }

        public Guid? LinkedToQuestionId { get; private set; }
        public Guid? LinkedToRosterId { get; private set; }
        public bool? IsInteger { get; private set; }

        public bool? AreAnswersOrdered { get; private set; }

        public bool? YesNoView { get; private set; }
        /// <summary>
        /// Gets or sets maximum count of answers for multioption question
        /// </summary>
        public int? MaxAllowedAnswers { get; private set; }
        public string Mask { get; private set; }
        public bool? IsFilteredCombobox { get; private set; }
        public Guid? CascadeFromQuestionId { get; private set; }
    }
}