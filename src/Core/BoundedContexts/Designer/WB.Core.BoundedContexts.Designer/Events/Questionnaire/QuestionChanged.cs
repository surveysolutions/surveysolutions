using WB.Core.BoundedContexts.Designer.Events.Questionnaire;

namespace Main.Core.Events.Questionnaire
{
    using System;
    using System.Collections.Generic;

    using Main.Core.Entities.SubEntities;

    using Ncqrs.Eventing.Storage;

    [EventName("RavenQuestionnaire.Core:Events:QuestionChangeded")]
    public class QuestionChanged : AbstractQuestionDataEvent
    {
        public Order AnswerOrder { get; set; }

        public Answer[] Answers { get; set; }

        public Guid TargetGroupKey { get; set; }

        public List<Guid> Triggers { get; set; }

        public int MaxValue { get; set; }

        public Guid? LinkedToQuestionId { get; set; }

        public bool? IsInteger { get; set; }
    }
}