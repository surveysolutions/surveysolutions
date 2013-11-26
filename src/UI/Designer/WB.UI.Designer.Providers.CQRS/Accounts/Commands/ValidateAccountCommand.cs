using Ncqrs.Commanding.CommandExecution.Mapping.Attributes;
using System;

namespace WB.UI.Designer.Providers.CQRS.Accounts.Commands
{
    [Serializable]
    [MapsToAggregateRootMethod(typeof (AccountAR), "Validate")]
    public class ValidateAccountCommand : AccountCommandBase
    {
        public ValidateAccountCommand(Guid accountId)
            : base(accountId) {}
    }
}