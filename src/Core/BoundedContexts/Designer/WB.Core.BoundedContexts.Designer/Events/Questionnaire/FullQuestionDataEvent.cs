using System;
using System.Collections.Generic;
using Main.Core.Entities.SubEntities;
using WB.Core.BoundedContexts.Designer.Events.Questionnaire;

namespace Main.Core.Events.Questionnaire
{
    public class FullQuestionDataEvent : AbstractQuestionDataEvent
    {
        public Order AnswerOrder { get; set; }
        public Answer[] Answers { get; set; }
        public Guid? GroupPublicKey { get; set; }
        public List<Guid> Triggers { get; set; }
        public int MaxValue { get; set; }
        public Guid? LinkedToQuestionId { get; set; }
        public bool? IsInteger { get; set; }
    }
}