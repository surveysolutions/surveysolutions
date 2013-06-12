using System;
using Main.Core.Domain;
using Ncqrs.Commanding.CommandExecution.Mapping.Attributes;

namespace Main.Core.Commands.Sync
{
    [Serializable]
    [MapsToAggregateRootConstructor(typeof(SyncActivityAR))]
    public class CreateSyncActivity
    {

    }
}
