using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ncqrs.Commanding;
using Ncqrs.Commanding.CommandExecution.Mapping.Attributes;
using RavenQuestionnaire.Core.Domain;
using RavenQuestionnaire.Core.Entities.SubEntities;

namespace RavenQuestionnaire.Core.Commands.Synchronization
{
    [Serializable]
    [MapsToAggregateRootConstructor(typeof(SyncProcessAR))]
    public class CreateNewSynchronizationProcessCommand : CommandBase
    {
        public Guid PublicKey { get; set; }
        public CreateNewSynchronizationProcessCommand(Guid publicKey)
        {
            this.PublicKey = publicKey;
        }
    }
}
