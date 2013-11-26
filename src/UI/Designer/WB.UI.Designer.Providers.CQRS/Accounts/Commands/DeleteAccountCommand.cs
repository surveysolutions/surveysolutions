using Ncqrs.Commanding.CommandExecution.Mapping.Attributes;
using System;

namespace WB.UI.Designer.Providers.CQRS.Accounts.Commands
{
    [Serializable]
    [MapsToAggregateRootMethod(typeof (AccountAR), "Delete")]
    public class DeleteAccountCommand : AccountCommandBase
    {
        public DeleteAccountCommand(Guid accountId)
            : base(accountId) {}
    }
}