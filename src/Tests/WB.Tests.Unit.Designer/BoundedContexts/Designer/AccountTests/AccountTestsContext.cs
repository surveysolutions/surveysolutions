using System;
using Machine.Specifications;
using WB.Core.BoundedContexts.Designer.Aggregates;

namespace WB.Tests.Unit.Designer.BoundedContexts.Designer.AccountTests
{
    [Subject(typeof(User))]
    internal class AccountTestsContext
    {
        public static User CreateAccount(Guid accountId, bool isConfirmed = false)
        {
            var accountAR = new User();

            accountAR.SetId(accountId);
            if (isConfirmed)
            {
                accountAR.Confirm();
            }

            return accountAR;
        }
    }
}
