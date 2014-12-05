using System;

namespace WB.Core.BoundedContexts.Designer.Commands.Account
{
    [Serializable]
    public class ConfirmAccountCommand : AccountCommandBase
    {
        public ConfirmAccountCommand(Guid accountId)
            : base(accountId) {}
    }
}