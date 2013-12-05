using System;
using Ncqrs.Commanding.CommandExecution.Mapping.Attributes;
using WB.Core.BoundedContexts.Designer.Aggregates;

namespace WB.Core.BoundedContexts.Designer.Commands.Account
{
    [Serializable]
    [MapsToAggregateRootMethod(typeof (AccountAR), "Validate")]
    public class ValidateAccountCommand : AccountCommandBase
    {
        public ValidateAccountCommand(Guid accountId)
            : base(accountId) {}
    }
}