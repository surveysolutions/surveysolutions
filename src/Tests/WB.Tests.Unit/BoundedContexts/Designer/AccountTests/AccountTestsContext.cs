using System;
using Machine.Specifications;
using WB.Core.BoundedContexts.Designer.Aggregates;

namespace WB.Tests.Unit.BoundedContexts.Designer.AccountTests
{
    [Subject(typeof(AccountAR))]
    internal class AccountTestsContext
    {
        public static AccountAR CreateAccount(Guid accountId, bool isConfirmed = false)
        {
            var accountAR = new AccountAR();

            accountAR.SetId(accountId);
            if (isConfirmed)
            {
                accountAR.Confirm();
            }

            return accountAR;
        }
    }
}
