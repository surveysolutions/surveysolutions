namespace Main.Core.Commands.User
{
    using System;

    using Main.Core.Domain;

    using Ncqrs.Commanding;
    using Ncqrs.Commanding.CommandExecution.Mapping.Attributes;

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