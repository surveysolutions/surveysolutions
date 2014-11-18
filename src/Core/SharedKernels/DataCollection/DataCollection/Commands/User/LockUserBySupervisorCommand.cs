using System;
using Ncqrs.Commanding;

namespace WB.Core.SharedKernels.DataCollection.Commands.User
{
    public class LockUserBySupervisorCommand : CommandBase
    {
        public LockUserBySupervisorCommand() {}

        public LockUserBySupervisorCommand(Guid userId)
        {
            this.UserId = userId;
        }

        public Guid UserId { get; set; }
    }
}