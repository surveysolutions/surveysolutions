using System;
using Ncqrs.Eventing.Storage;

namespace RavenQuestionnaire.Core.Events
{
    [Serializable]
    [EventName("RavenQuestionnaire.Core:Events:NewCompleteQuestionnaireCreated")]
    public class NewCompleteQuestionnaireCreated
    {
        public Guid CompletedQuestionnaireId { get; set; }
        public Guid QuestionnaireId { get; set; }
        public string QuestionnaireIdOld { get; set; }
        
        public DateTime CreationDate { get; set; }
        
    }
}
