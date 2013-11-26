using Ncqrs.Commanding.CommandExecution.Mapping.Attributes;
using System;

namespace WB.UI.Designer.Providers.CQRS.Accounts.Commands
{
    using WB.UI.Shared.Web.MembershipProvider.Roles;

    [Serializable]
    [MapsToAggregateRootMethod(typeof (AccountAR), "RemoveRole")]
    public class RemoveRoleFromAccountCommand : AccountCommandBase
    {
        public RemoveRoleFromAccountCommand(Guid accountId, SimpleRoleEnum role)
            : base(accountId)
        {
            Role = role;
        }

        public SimpleRoleEnum Role { get; private set; }
    }
}