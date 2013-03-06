using Ncqrs.Commanding;
using Ncqrs.Commanding.CommandExecution.Mapping.Attributes;
using System;

namespace Designer.Web.Providers.CQRS.Accounts.Commands
{
    [Serializable]
    [MapsToAggregateRootMethod(typeof(AccountAR), "Unlock")]
    public class UnlockAccountCommand : CommandBase
    {
        public UnlockAccountCommand() {}

        public UnlockAccountCommand(Guid publicKey)
        {
            PublicKey = publicKey;
        }

        [AggregateRootId]
        public Guid PublicKey { get; set; }
    }
}