using System;

namespace WB.Core.BoundedContexts.Designer.Commands.Account
{
    [Serializable]
    public class DeleteAccountCommand : AccountCommandBase
    {
        public DeleteAccountCommand(Guid accountId)
            : base(accountId) {}
    }
}