using Ncqrs.Commanding.CommandExecution.Mapping.Attributes;
using System;

namespace WB.UI.Designer.Providers.CQRS.Accounts.Commands
{
    [Serializable]
    [MapsToAggregateRootMethod(typeof (AccountAR), "ChangePassword")]
    public class ChangePasswordAccountCommand : AccountCommandBase
    {
        public ChangePasswordAccountCommand(Guid accountId, string password)
            : base(accountId)
        {
            Password = password;
        }

        public string Password { get; private set; }
    }
}