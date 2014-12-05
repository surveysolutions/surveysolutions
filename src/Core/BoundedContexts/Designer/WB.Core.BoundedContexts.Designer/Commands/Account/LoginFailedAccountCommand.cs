using System;
using WB.Core.BoundedContexts.Designer.Aggregates;

namespace WB.Core.BoundedContexts.Designer.Commands.Account
{
    [Serializable]
    public class LoginFailedAccountCommand : AccountCommandBase
    {
        public LoginFailedAccountCommand(Guid accountId)
            : base(accountId) {}
    }
}
