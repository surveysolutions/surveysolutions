using System;

namespace WB.Core.BoundedContexts.Designer.Commands.Account
{
    [Serializable]
    public class ChangeOnlineAccountCommand : AccountCommandBase
    {
        public ChangeOnlineAccountCommand(Guid accountId)
            : base(accountId) {}
    }
}