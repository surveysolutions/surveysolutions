using System;
using Ncqrs.Commanding.CommandExecution.Mapping.Attributes;
using WB.Core.BoundedContexts.Designer.Aggregates;

namespace WB.Core.BoundedContexts.Designer.Commands.Account
{
    [Serializable]
    [MapsToAggregateRootMethod(typeof (AccountAR), "LoginFailed")]
    public class LoginFailedAccountCommand : AccountCommandBase
    {
        public LoginFailedAccountCommand(Guid accountId)
            : base(accountId) {}
    }
}
