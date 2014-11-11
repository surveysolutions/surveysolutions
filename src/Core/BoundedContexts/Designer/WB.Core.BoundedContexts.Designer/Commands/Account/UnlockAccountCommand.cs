using System;

namespace WB.Core.BoundedContexts.Designer.Commands.Account
{
    [Serializable]
    public class UnlockAccountCommand : AccountCommandBase
    {
        public UnlockAccountCommand(Guid accountId)
            : base(accountId) {}
    }
}