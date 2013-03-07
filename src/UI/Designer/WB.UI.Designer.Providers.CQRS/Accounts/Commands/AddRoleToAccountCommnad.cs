using WB.UI.Designer.Providers.Roles;
using Ncqrs.Commanding;
using Ncqrs.Commanding.CommandExecution.Mapping.Attributes;
using System;

namespace WB.UI.Designer.Providers.CQRS.Accounts.Commands
{
    using WB.UI.Designer.Providers.Roles;

    [Serializable]
    [MapsToAggregateRootMethod(typeof(AccountAR), "AddRole")]
    public class AddRoleToAccountCommnad : CommandBase
    {
        public AddRoleToAccountCommnad() {}

        public AddRoleToAccountCommnad(Guid accountPublicKey, SimpleRoleEnum role)
        {
            AccountPublicKey = accountPublicKey;
            Role = role;
        }

        [AggregateRootId]
        public Guid AccountPublicKey { get; set; }
        public SimpleRoleEnum Role { get; set; }
    }
}