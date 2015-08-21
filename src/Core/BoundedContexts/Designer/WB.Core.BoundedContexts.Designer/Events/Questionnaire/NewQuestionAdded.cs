namespace Main.Core.Events.Questionnaire
{
    using System;
    using Ncqrs.Eventing.Storage;

    /// <summary>
    /// The new question added.
    /// </summary>
    public class NewQuestionAdded : FullQuestionDataEvent
    {
    }
}