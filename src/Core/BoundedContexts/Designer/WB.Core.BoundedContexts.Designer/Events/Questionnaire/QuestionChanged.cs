using WB.Core.BoundedContexts.Designer.Events.Questionnaire;

namespace Main.Core.Events.Questionnaire
{
    using System;
    using System.Collections.Generic;

    using Main.Core.Entities.SubEntities;

    using Ncqrs.Eventing.Storage;

    [EventName("RavenQuestionnaire.Core:Events:QuestionChangeded")]
    public class QuestionChanged : FullQuestionDataEvent
    {
        public Guid TargetGroupKey { get; set; }
    }
}