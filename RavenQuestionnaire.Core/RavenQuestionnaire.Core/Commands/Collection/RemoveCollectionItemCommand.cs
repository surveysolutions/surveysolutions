using System;
using Ncqrs.Commanding;
using Ncqrs.Commanding.CommandExecution.Mapping.Attributes;
using RavenQuestionnaire.Core.Domain;

namespace RavenQuestionnaire.Core.Commands.Collection
{
    [Serializable]
    [MapsToAggregateRootMethod(typeof(CollectionAR), "RemoveCollectionItem")]
    public class RemoveCollectionItemCommand : CommandBase
    {
        public Guid CollectionId { get; set; }
    }
}
