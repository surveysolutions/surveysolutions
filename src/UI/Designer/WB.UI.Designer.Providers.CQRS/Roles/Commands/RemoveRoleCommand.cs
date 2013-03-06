using WB.UI.Designer.Providers.CQRS.Accounts;
using Ncqrs.Commanding;
using Ncqrs.Commanding.CommandExecution.Mapping.Attributes;
using System;

namespace WB.UI.Designer.Providers.CQRS.Roles.Commands
{
    using WB.UI.Designer.Providers.CQRS.Accounts;

    [Serializable]
    [MapsToAggregateRootMethod(typeof(AccountAR), "Remove")]
    public class RemoveRoleCommand : CommandBase
    {
        public RemoveRoleCommand() {}

        public RemoveRoleCommand(Guid publicKey)
        {
            PublicKey = publicKey;
        }

        [AggregateRootId]
        public Guid PublicKey { get; set; }
    }
}