using Ncqrs.Commanding;
using Ncqrs.Commanding.CommandExecution.Mapping.Attributes;
using System;

namespace Designer.Web.Providers.CQRS.Accounts.Commands
{
    [Serializable]
    [MapsToAggregateRootMethod(typeof(AccountAR), "ResetPassword")]
    public class ResetPasswordAccountCommand : CommandBase
    {
        public ResetPasswordAccountCommand() {}

        public ResetPasswordAccountCommand(Guid publicKey, string password, string passwordSalt)
        {
            PublicKey = publicKey;
            Password = password;
            PasswordSalt = passwordSalt;
        }

        [AggregateRootId]
        public Guid PublicKey { get; set; }

        public string Password { get; set; }
        public string PasswordSalt { get; set; }
    }
}