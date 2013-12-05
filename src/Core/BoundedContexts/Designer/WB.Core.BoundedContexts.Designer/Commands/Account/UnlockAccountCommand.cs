using System;
using Ncqrs.Commanding.CommandExecution.Mapping.Attributes;
using WB.Core.BoundedContexts.Designer.Aggregates;

namespace WB.Core.BoundedContexts.Designer.Commands.Account
{
    [Serializable]
    [MapsToAggregateRootMethod(typeof (AccountAR), "Unlock")]
    public class UnlockAccountCommand : AccountCommandBase
    {
        public UnlockAccountCommand(Guid accountId)
            : base(accountId) {}
    }
}