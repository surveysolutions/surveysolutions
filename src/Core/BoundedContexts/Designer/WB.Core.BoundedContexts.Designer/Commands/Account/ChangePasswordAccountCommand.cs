using System;
using Ncqrs.Commanding.CommandExecution.Mapping.Attributes;
using WB.Core.BoundedContexts.Designer.Aggregates;

namespace WB.Core.BoundedContexts.Designer.Commands.Account
{
    [Serializable]
    [MapsToAggregateRootMethod(typeof (AccountAR), "ChangePassword")]
    public class ChangePasswordAccountCommand : AccountCommandBase
    {
        public ChangePasswordAccountCommand(Guid accountId, string password)
            : base(accountId)
        {
            this.Password = password;
        }

        public string Password { get; private set; }
    }
}