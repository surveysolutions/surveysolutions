using Ncqrs.Commanding.CommandExecution.Mapping.Attributes;
using System;

namespace WB.UI.Designer.Providers.CQRS.Accounts.Commands
{
    [Serializable]
    [MapsToAggregateRootMethod(typeof (AccountAR), "ResetPassword")]
    public class ResetPasswordAccountCommand : AccountCommandBase
    {
        public ResetPasswordAccountCommand(Guid accountId, string password, string passwordSalt)
            : base(accountId)
        {
            Password = password;
            PasswordSalt = passwordSalt;
        }

        public string Password { get; private set; }
        public string PasswordSalt { get; private set; }
    }
}