using System;
using Ncqrs.Eventing.Storage;

namespace RavenQuestionnaire.Core.Events
{
    [Serializable]
    [EventName("RavenQuestionnaire.Core:Events:NewLocationCreated")]
    public class NewLocationCreated
    {
        public Guid LocationId { get; set; }
        public string Title { get; set; }

        public DateTime CreationDate { get; set; }
    }
}
