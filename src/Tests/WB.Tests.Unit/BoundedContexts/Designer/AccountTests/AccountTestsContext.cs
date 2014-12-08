using System;
using Machine.Specifications;
using WB.Core.BoundedContexts.Designer.Aggregates;

namespace WB.Tests.Unit.BoundedContexts.Designer.AccountTests
{
    [Subject(typeof(AccountAR))]
    public class AccountTestsContext
    {
        public static AccountAR CreateAccount(Guid accountId, string applicationName = null, string userName = null, string email = null, string password = null, string passwordSalt = null,
            bool isConfirmed = false, string confirmationToken = null)
        {
            return new AccountAR(applicationName: applicationName, userName: userName, email: email, accountId: accountId,
                password: password, passwordSalt: passwordSalt, isConfirmed: isConfirmed, confirmationToken: confirmationToken);
        }
    }
}
