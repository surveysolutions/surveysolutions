using Ncqrs.Commanding;
using Ncqrs.Commanding.CommandExecution.Mapping.Attributes;
using System;

namespace Designer.Web.Providers.CQRS.Accounts.Commands
{
    [Serializable]
    [MapsToAggregateRootMethod(typeof(AccountAR), "Validate")]
    public class ValidateAccountCommand : CommandBase
    {
        public ValidateAccountCommand() {}

        public ValidateAccountCommand(Guid publicKey)
        {
            PublicKey = publicKey;
        }

        [AggregateRootId]
        public Guid PublicKey { get; set; }
    }
}