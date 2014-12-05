using System;

namespace WB.Core.BoundedContexts.Designer.Commands.Account
{
    [Serializable]
    public class LockAccountCommand : AccountCommandBase
    {
        public LockAccountCommand(Guid accountId)
            : base(accountId) {}
    }
}