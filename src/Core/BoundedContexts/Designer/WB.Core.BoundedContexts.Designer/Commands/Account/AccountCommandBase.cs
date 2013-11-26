using System;
using Ncqrs.Commanding;
using Ncqrs.Commanding.CommandExecution.Mapping.Attributes;

namespace WB.Core.BoundedContexts.Designer.Commands.Account
{
    public class AccountCommandBase : CommandBase
    {
        public AccountCommandBase(Guid accountId)
        {
            this.AccountId = accountId;
        }

        [AggregateRootId]
        public Guid AccountId { get; private set; }
    }
}
