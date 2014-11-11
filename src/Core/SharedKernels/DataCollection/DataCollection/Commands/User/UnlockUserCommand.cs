using System;
using Ncqrs.Commanding;

namespace WB.Core.SharedKernels.DataCollection.Commands.User
{
    [Serializable]
    public class UnlockUserCommand : CommandBase
    {
        public UnlockUserCommand() { }

        public UnlockUserCommand(Guid publicKey)
        {
            this.PublicKey = publicKey;
        }

        public Guid PublicKey { get; set; }
    }
}