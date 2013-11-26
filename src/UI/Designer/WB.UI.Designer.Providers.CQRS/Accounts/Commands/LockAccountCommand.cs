using Ncqrs.Commanding.CommandExecution.Mapping.Attributes;
using System;

namespace WB.UI.Designer.Providers.CQRS.Accounts.Commands
{
    [Serializable]
    [MapsToAggregateRootMethod(typeof (AccountAR), "Lock")]
    public class LockAccountCommand : AccountCommandBase
    {
        public LockAccountCommand(Guid accountId)
            : base(accountId) {}
    }
}