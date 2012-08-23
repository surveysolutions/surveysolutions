using System;
using Ncqrs.Eventing.Storage;

namespace RavenQuestionnaire.Core.Events.Questionnaire
{
    [Serializable]
    [EventName("RavenQuestionnaire.Core:Events:QuestionDeleted")]
    public class QuestionDeleted
    {
        public Guid QuestionId { get; set; }
    }
}
