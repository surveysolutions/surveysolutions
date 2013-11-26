using System;
using Ncqrs.Commanding.CommandExecution.Mapping.Attributes;
using WB.Core.BoundedContexts.Designer.Aggregates;

namespace WB.Core.BoundedContexts.Designer.Commands.Account
{
    [Serializable]
    [MapsToAggregateRootMethod(typeof (AccountAR), "Delete")]
    public class DeleteAccountCommand : AccountCommandBase
    {
        public DeleteAccountCommand(Guid accountId)
            : base(accountId) {}
    }
}