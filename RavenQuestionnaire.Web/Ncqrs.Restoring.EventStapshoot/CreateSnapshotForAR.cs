using System;
using Ncqrs.Commanding;
using Ncqrs.Commanding.CommandExecution.Mapping.Attributes;
using Ncqrs.Domain;

namespace Ncqrs.Restoring.EventStapshoot
{
    [Serializable]
    public class CreateSnapshotForAR/*<T>*/ : CommandBase //where T:AggregateRoot
    {

        public CreateSnapshotForAR(Guid aggregateRootId, Type aggregateRootType )
        {
            this.AggregateRootId = aggregateRootId;
            this.AggregateRootType = aggregateRootType;
        }

        /// <summary>
        /// Gets or sets the questionnaire id.
        /// </summary>
        [AggregateRootId]
        public Guid AggregateRootId { get; set; }

        public Type AggregateRootType { get; set; }
    }
}