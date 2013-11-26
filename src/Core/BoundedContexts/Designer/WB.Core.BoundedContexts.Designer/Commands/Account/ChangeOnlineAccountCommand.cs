using System;
using Ncqrs.Commanding.CommandExecution.Mapping.Attributes;
using WB.Core.BoundedContexts.Designer.Aggregates;

namespace WB.Core.BoundedContexts.Designer.Commands.Account
{
    [Serializable]
    [MapsToAggregateRootMethod(typeof (AccountAR), "ChangeOnline")]
    public class ChangeOnlineAccountCommand : AccountCommandBase
    {
        public ChangeOnlineAccountCommand(Guid accountId)
            : base(accountId) {}
    }
}