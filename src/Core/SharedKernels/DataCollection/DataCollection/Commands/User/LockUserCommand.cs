using System;
using Ncqrs.Commanding;

namespace WB.Core.SharedKernels.DataCollection.Commands.User
{
    [Serializable]
    public class LockUserCommand : CommandBase
    {
        public LockUserCommand() {}

        public LockUserCommand(Guid publicKey)
        {
            this.PublicKey = publicKey;
        }

        public Guid PublicKey { get; set; }
    }
}