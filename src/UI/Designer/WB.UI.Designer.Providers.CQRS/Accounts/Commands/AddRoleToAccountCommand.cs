using Ncqrs.Commanding.CommandExecution.Mapping.Attributes;
using System;

namespace WB.UI.Designer.Providers.CQRS.Accounts.Commands
{
    using WB.UI.Shared.Web.MembershipProvider.Roles;

    [Serializable]
    [MapsToAggregateRootMethod(typeof (AccountAR), "AddRole")]
    public class AddRoleToAccountCommand : AccountCommandBase
    {
        public AddRoleToAccountCommand(Guid accountId, SimpleRoleEnum role)
            : base(accountId)
        {
            Role = role;
        }

        public SimpleRoleEnum Role { get; private set; }
    }
}