using System;
using Machine.Specifications;
using WB.Core.BoundedContexts.Designer.Aggregates;

namespace WB.Tests.Unit.Designer.BoundedContexts.Designer.AccountTests
{
    [Subject(typeof(Account))]
    internal class AccountTestsContext
    {
        public static Account CreateAccount(Guid accountId, bool isConfirmed = false)
        {
            var accountAR = new Account();

            accountAR.SetId(accountId);
            if (isConfirmed)
            {
                accountAR.Confirm();
            }

            return accountAR;
        }
    }
}
