using System;
using Ncqrs.Commanding;
using Ncqrs.Commanding.CommandExecution.Mapping.Attributes;

namespace WB.UI.Designer.Providers.CQRS.Accounts.Commands
{
    public class AccountCommandBase : CommandBase
    {
        public AccountCommandBase(Guid accountId)
        {
            AccountId = accountId;
        }

        [AggregateRootId]
        public Guid AccountId { get; private set; }
    }
}
