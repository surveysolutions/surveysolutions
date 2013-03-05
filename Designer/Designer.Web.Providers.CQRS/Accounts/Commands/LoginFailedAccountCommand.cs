using Ncqrs.Commanding;
using Ncqrs.Commanding.CommandExecution.Mapping.Attributes;
using System;

namespace Designer.Web.Providers.CQRS.Accounts.Commands
{
    [Serializable]
    [MapsToAggregateRootMethod(typeof(AccountAR), "LoginFailed")]
    public class LoginFailedAccountCommand : CommandBase
    {
        public LoginFailedAccountCommand() { }

        public LoginFailedAccountCommand(Guid publicKey)
        {
            PublicKey = publicKey;
        }

        [AggregateRootId]
        public Guid PublicKey { get; set; }
    }
}
