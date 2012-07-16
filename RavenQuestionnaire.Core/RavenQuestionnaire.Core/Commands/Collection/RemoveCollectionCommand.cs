using System;
using Ncqrs.Commanding;
using Ncqrs.Commanding.CommandExecution.Mapping.Attributes;
using RavenQuestionnaire.Core.Domain;

namespace RavenQuestionnaire.Core.Commands.Collection
{

    [Serializable]
    [MapsToAggregateRootMethod(typeof(CollectionAR), "RemoveCollection")]
    public class RemoveCollectionCommand : CommandBase
    {
        [AggregateRootId]
        public Guid CollectionId { get; set; }

    }
}
