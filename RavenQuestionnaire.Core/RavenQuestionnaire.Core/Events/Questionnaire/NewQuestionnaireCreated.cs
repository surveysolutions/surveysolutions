using System;
using Ncqrs.Eventing.Storage;

namespace RavenQuestionnaire.Core.Events
{
    [Serializable]
    [EventName("RavenQuestionnaire.Core:Events:NewQuestionnaireCreated")]
    public class NewQuestionnaireCreated
    {
        public Guid PublicKey { set; get; }

        public string Title { get; set; }
        public DateTime CreationDate { get; set; }
    }
}
