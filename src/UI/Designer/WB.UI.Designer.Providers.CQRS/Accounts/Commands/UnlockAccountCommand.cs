using Ncqrs.Commanding.CommandExecution.Mapping.Attributes;
using System;

namespace WB.UI.Designer.Providers.CQRS.Accounts.Commands
{
    [Serializable]
    [MapsToAggregateRootMethod(typeof (AccountAR), "Unlock")]
    public class UnlockAccountCommand : AccountCommandBase
    {
        public UnlockAccountCommand(Guid accountId)
            : base(accountId) {}
    }
}