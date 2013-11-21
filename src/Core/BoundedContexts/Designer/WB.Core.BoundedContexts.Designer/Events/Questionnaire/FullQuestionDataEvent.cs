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
        public List<Guid> Triggers { get; set; }

        [Obsolete("Property is obsolete, actual only for old AutoPropagate question, had default value 10.")]
        public int MaxValue { get; private set; }

        public Guid? LinkedToQuestionId { get; set; }
        public bool? IsInteger { get; set; }

        public bool? AreAnswersOrdered { get; set; }
        public int? MaxAllowedAnswers { get; set; }
    }
}