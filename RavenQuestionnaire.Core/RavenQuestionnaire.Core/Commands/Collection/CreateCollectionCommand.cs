using System;
using System.Collections.Generic;
using Ncqrs.Commanding;
using Ncqrs.Commanding.CommandExecution.Mapping.Attributes;
using RavenQuestionnaire.Core.Domain;
using RavenQuestionnaire.Core.Entities.SubEntities;

namespace RavenQuestionnaire.Core.Commands.Collection
{
    [Serializable]
    [MapsToAggregateRootConstructor(typeof(CollectionAR))]
    public class CreateCollectionCommand : CommandBase
    {
        public Guid CollectionId { get; set; }

        public String Text { get; set; }

        public List<CollectionItem> Items { get; set; }

        public CreateCollectionCommand(Guid collectionId, string title, List<CollectionItem> items)
        {
            CollectionId = collectionId;
            Text = title;
            Items = items;
        }
    }
}
