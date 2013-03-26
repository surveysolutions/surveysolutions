using Ncqrs.Commanding;
using Ncqrs.Commanding.CommandExecution.Mapping.Attributes;
using System;

namespace WB.UI.Designer.Providers.CQRS.Accounts.Commands
{
    [Serializable]
    [MapsToAggregateRootMethod(typeof(AccountAR), "ChangePassword")]
    public class ChangePasswordAccountCommand : CommandBase
    {
        public ChangePasswordAccountCommand() {}

        public ChangePasswordAccountCommand(Guid publicKey, string password)
        {
            PublicKey = publicKey;
            Password = password;
        }

        [AggregateRootId]
        public Guid PublicKey { get; set; }

        public string Password { get; set; }
    }
}