using System;
using Ncqrs.Commanding;

namespace WB.Core.SharedKernels.DataCollection.Commands.User
{
    [Serializable]
    public class UnlockUserBySupervisorCommand : CommandBase
    {
        public UnlockUserBySupervisorCommand() { }

        public UnlockUserBySupervisorCommand(Guid publicKey)
        {
            this.PublicKey = publicKey;
        }

        public Guid PublicKey { get; set; }
    }
}