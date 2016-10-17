using System;
using Machine.Specifications;
using WB.Core.BoundedContexts.Designer.Aggregates;
using WB.UI.Designer.Providers.CQRS.Accounts.Events;

namespace WB.Tests.Unit.Designer.BoundedContexts.Designer.AccountTests
{
    [Ignore("KP-7922 KP-7923")]
    internal class when_updating_unlocked_account_and_specifying_is_locked_true : AccountTestsContext
    {
        Establish context = () =>
        {
            var accountId = Guid.Parse("11111111111111111111111111111111");

            user = CreateAccount(accountId);

            eventContext = new EventContext();
        };

        Because of = () =>
            user.Update(userName: null, comment: null, email: null, passwordQuestion: null, isLockedOut: true, isConfirmed: false);

        Cleanup stuff = () =>
        {
            eventContext.Dispose();
            eventContext = null;
        };

        It should_raise_AccountLocked = () =>
            eventContext.ShouldContainEvent<AccountLocked>();

        private static EventContext eventContext;
        private static User user;
    }
}