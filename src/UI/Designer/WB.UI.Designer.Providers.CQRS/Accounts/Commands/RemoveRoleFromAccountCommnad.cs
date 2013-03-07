using WB.UI.Designer.Providers.Roles;
using Ncqrs.Commanding;
using Ncqrs.Commanding.CommandExecution.Mapping.Attributes;
using System;

namespace WB.UI.Designer.Providers.CQRS.Accounts.Commands
{
    using WB.UI.Designer.Providers.Roles;

    [Serializable]
    [MapsToAggregateRootMethod(typeof(AccountAR), "RemoveRole")]
    public class RemoveRoleFromAccountCommnad : CommandBase
    {
        public RemoveRoleFromAccountCommnad() {}

        public RemoveRoleFromAccountCommnad(Guid accountPublicKey, SimpleRoleEnum role)
        {
            AccountPublicKey = accountPublicKey;
            Role = role;
        }


        public SimpleRoleEnum Role { get; set; }

        [AggregateRootId]
        public Guid AccountPublicKey { get; set; }
    }
}