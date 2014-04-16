using System;
using Ncqrs.Commanding;
using Ncqrs.Commanding.CommandExecution.Mapping.Attributes;
using WB.Core.SharedKernels.DataCollection.Aggregates;

namespace WB.Core.SharedKernels.DataCollection.Commands.User
{
    [Serializable]
    [MapsToAggregateRootMethod(typeof(UserAR), "UnlockBySupervisor")]
    public class UnlockUserBySupervisorCommand : CommandBase
    {
        public UnlockUserBySupervisorCommand() { }

        public UnlockUserBySupervisorCommand(Guid publicKey)
        {
            this.PublicKey = publicKey;
        }

        [AggregateRootId]
        public Guid PublicKey { get; set; }
    }
}