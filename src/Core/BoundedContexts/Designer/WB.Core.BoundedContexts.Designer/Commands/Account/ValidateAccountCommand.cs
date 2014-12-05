using System;

namespace WB.Core.BoundedContexts.Designer.Commands.Account
{
    [Serializable]
    public class ValidateAccountCommand : AccountCommandBase
    {
        public ValidateAccountCommand(Guid accountId)
            : base(accountId) {}
    }
}