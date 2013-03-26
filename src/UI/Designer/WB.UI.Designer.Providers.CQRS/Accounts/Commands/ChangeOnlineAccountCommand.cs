using Ncqrs.Commanding;
using Ncqrs.Commanding.CommandExecution.Mapping.Attributes;
using System;

namespace WB.UI.Designer.Providers.CQRS.Accounts.Commands
{
    [Serializable]
    [MapsToAggregateRootMethod(typeof(AccountAR), "ChangeOnline")]
    public class ChangeOnlineAccountCommand : CommandBase
    {
        public ChangeOnlineAccountCommand() {}

        public ChangeOnlineAccountCommand(Guid publicKey)
        {
            PublicKey = publicKey;
        }

        [AggregateRootId]
        public Guid PublicKey { get; set; }
    }
}