namespace Main.Core.Events.Questionnaire
{
    using System;
    using System.Collections.Generic;

    using Main.Core.Entities.SubEntities;

    using Ncqrs.Eventing.Storage;

    /// <summary>
    /// The new question added.
    /// </summary>
    [EventName("RavenQuestionnaire.Core:Events:QuestionCloned")]
    public class QuestionCloned : FullQuestionDataEvent
    {
        public Guid SourceQuestionId { get; set; }

        public int TargetIndex { get; set; }
    }
}