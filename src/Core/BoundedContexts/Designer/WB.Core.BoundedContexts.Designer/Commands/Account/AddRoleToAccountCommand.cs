using System;
using Ncqrs.Commanding.CommandExecution.Mapping.Attributes;
using WB.Core.BoundedContexts.Designer.Aggregates;
using WB.UI.Shared.Web.MembershipProvider.Roles;

namespace WB.Core.BoundedContexts.Designer.Commands.Account
{
    [Serializable]
    [MapsToAggregateRootMethod(typeof (AccountAR), "AddRole")]
    public class AddRoleToAccountCommand : AccountCommandBase
    {
        public AddRoleToAccountCommand(Guid accountId, SimpleRoleEnum role)
            : base(accountId)
        {
            this.Role = role;
        }

        public SimpleRoleEnum Role { get; private set; }
    }
}