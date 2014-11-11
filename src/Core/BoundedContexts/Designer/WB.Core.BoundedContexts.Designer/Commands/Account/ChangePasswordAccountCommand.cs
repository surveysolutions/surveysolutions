using System;

namespace WB.Core.BoundedContexts.Designer.Commands.Account
{
    [Serializable]
    public class ChangePasswordAccountCommand : AccountCommandBase
    {
        public ChangePasswordAccountCommand(Guid accountId, string password)
            : base(accountId)
        {
            this.Password = password;
        }

        public string Password { get; private set; }
    }
}