using System;
using Ncqrs.Commanding;
using Ncqrs.Commanding.CommandExecution.Mapping.Attributes;
using WB.Core.SharedKernels.DataCollection.Aggregates;

namespace WB.Core.SharedKernels.DataCollection.Commands.User
{
    [Serializable]
    [MapsToAggregateRootMethod(typeof(UserAR), "LockBySupervisor")]
    public class LockUserBySupervisorCommand : CommandBase
    {
        public LockUserBySupervisorCommand() {}

        public LockUserBySupervisorCommand(Guid userId)
        {
            this.UserId = userId;
        }

        [AggregateRootId]
        public Guid UserId { get; set; }
    }
}