using Ncqrs.Commanding.CommandExecution.Mapping.Attributes;
using System;

namespace WB.UI.Designer.Providers.CQRS.Accounts.Commands
{
    [Serializable]
    [MapsToAggregateRootMethod(typeof (AccountAR), "ChangeOnline")]
    public class ChangeOnlineAccountCommand : AccountCommandBase
    {
        public ChangeOnlineAccountCommand(Guid accountId)
            : base(accountId) {}
    }
}