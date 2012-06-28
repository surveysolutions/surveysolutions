using System;
using System.Collections.Generic;
using Ncqrs.Eventing.Storage;
using RavenQuestionnaire.Core.Entities.SubEntities;

namespace RavenQuestionnaire.Core.Events
{
    [Serializable]
    [EventName("RavenQuestionnaire.Core:Events:NewCollectionCreated")]
    public class NewCollectionCreated
    {
        public Guid CollectionId { get; set; }
        public string Title { get; set; }
        public List<CollectionItem> Items { set; get; }
        public DateTime CreationDate { get; set; }
    }
}
