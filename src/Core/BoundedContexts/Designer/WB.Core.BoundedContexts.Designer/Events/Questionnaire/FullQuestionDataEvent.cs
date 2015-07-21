using System;
using System.Collections.Generic;
using Main.Core.Entities.SubEntities;
using WB.Core.BoundedContexts.Designer.Events.Questionnaire;

namespace Main.Core.Events.Questionnaire
{
    public class FullQuestionDataEvent : AbstractQuestionDataEvent
    {
        public QuestionType QuestionType { get; set; }
        public Order AnswerOrder { get; set; }
        public Answer[] Answers { get; set; }
        public Guid? GroupPublicKey { get; set; }

        public Guid? LinkedToQuestionId { get; set; }
        public bool? IsInteger { get; set; }

        public bool? AreAnswersOrdered { get; set; }

        /// <summary>
        /// Gets or sets maximum count of answers for multioption question
        /// </summary>
        public int? MaxAllowedAnswers { get; set; }
        public string Mask { get; set; }
        public bool? IsFilteredCombobox { get; set; }
        public Guid? CascadeFromQuestionId { get; set; }
    }
}