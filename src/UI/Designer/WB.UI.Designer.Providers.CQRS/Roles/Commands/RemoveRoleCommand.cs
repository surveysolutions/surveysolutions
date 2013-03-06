using Designer.Web.Providers.CQRS.Accounts;
using Ncqrs.Commanding;
using Ncqrs.Commanding.CommandExecution.Mapping.Attributes;
using System;

namespace Designer.Web.Providers.CQRS.Roles.Commands
{
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