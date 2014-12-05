using System;
using Ncqrs.Commanding;

namespace WB.Core.BoundedContexts.Designer.Commands.Account
{
    public class AccountCommandBase : CommandBase
    {
        public AccountCommandBase(Guid accountId)
        {
            this.AccountId = accountId;
        }

        public Guid AccountId { get; private set; }
    }
}
