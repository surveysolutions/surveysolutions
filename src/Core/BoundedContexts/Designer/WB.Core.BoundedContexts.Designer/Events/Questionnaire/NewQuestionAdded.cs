namespace Main.Core.Events.Questionnaire
{
    using System;
    using Ncqrs.Eventing.Storage;

    /// <summary>
    /// The new question added.
    /// </summary>
    [EventName("RavenQuestionnaire.Core:Events:NewQuestionAdded")]
    public class NewQuestionAdded : FullQuestionDataEvent
    {
    }
}