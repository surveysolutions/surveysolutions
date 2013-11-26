using Ncqrs.Commanding.CommandExecution.Mapping.Attributes;
using System;

namespace WB.UI.Designer.Providers.CQRS.Accounts.Commands
{
    [Serializable]
    [MapsToAggregateRootMethod(typeof (AccountAR), "LoginFailed")]
    public class LoginFailedAccountCommand : AccountCommandBase
    {
        public LoginFailedAccountCommand(Guid accountId)
            : base(accountId) {}
    }
}
