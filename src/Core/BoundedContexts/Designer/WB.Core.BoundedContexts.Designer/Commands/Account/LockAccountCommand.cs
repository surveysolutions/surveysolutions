using System;
using Ncqrs.Commanding.CommandExecution.Mapping.Attributes;
using WB.Core.BoundedContexts.Designer.Aggregates;

namespace WB.Core.BoundedContexts.Designer.Commands.Account
{
    [Serializable]
    [MapsToAggregateRootMethod(typeof (AccountAR), "Lock")]
    public class LockAccountCommand : AccountCommandBase
    {
        public LockAccountCommand(Guid accountId)
            : base(accountId) {}
    }
}