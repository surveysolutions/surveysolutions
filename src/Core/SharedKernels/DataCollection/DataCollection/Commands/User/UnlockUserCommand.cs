using System;
using Main.Core.Domain;
using Ncqrs.Commanding;
using Ncqrs.Commanding.CommandExecution.Mapping.Attributes;
using WB.Core.SharedKernels.DataCollection.Aggregates;

namespace WB.Core.SharedKernels.DataCollection.Commands.User
{
    [Serializable]
    [MapsToAggregateRootMethod(typeof(UserAR), "Unlock")]
    public class UnlockUserCommand : CommandBase
    {
        public UnlockUserCommand() { }

        public UnlockUserCommand(Guid publicKey)
        {
            this.PublicKey = publicKey;
        }

        [AggregateRootId]
        public Guid PublicKey { get; set; }
    }
}