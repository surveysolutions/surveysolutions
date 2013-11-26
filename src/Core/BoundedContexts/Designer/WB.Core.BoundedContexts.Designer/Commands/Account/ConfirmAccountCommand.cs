using System;
using Ncqrs.Commanding.CommandExecution.Mapping.Attributes;
using WB.Core.BoundedContexts.Designer.Aggregates;

namespace WB.Core.BoundedContexts.Designer.Commands.Account
{
    [Serializable]
    [MapsToAggregateRootMethod(typeof (AccountAR), "Confirm")]
    public class ConfirmAccountCommand : AccountCommandBase
    {
        public ConfirmAccountCommand(Guid accountId)
            : base(accountId) {}
    }
}