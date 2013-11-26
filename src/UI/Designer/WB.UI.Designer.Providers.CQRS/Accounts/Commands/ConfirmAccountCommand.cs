using Ncqrs.Commanding.CommandExecution.Mapping.Attributes;
using System;

namespace WB.UI.Designer.Providers.CQRS.Accounts.Commands
{
    [Serializable]
    [MapsToAggregateRootMethod(typeof (AccountAR), "Confirm")]
    public class ConfirmAccountCommand : AccountCommandBase
    {
        public ConfirmAccountCommand(Guid accountId)
            : base(accountId) {}
    }
}