using System;
using Machine.Specifications;
using WB.Core.BoundedContexts.Designer.Aggregates;
using WB.UI.Designer.Providers.CQRS.Accounts.Events;

namespace WB.Tests.Unit.Designer.BoundedContexts.Designer.AccountTests
{
    [Ignore("KP-7922 KP-7923")]
    internal class when_updating_confirmed_account_and_specifying_is_confirmed_true : AccountTestsContext
    {
        Establish context = () =>
        {
            var accountId = Guid.Parse("11111111111111111111111111111111");

            account = CreateAccount(accountId: accountId, isConfirmed: true);

            eventContext = new EventContext();
        };

        Because of = () =>
            account.Update(userName: "user name", comment: "some comment", email: "user@e.mail", passwordQuestion: "secret question", isLockedOut: true,
                isConfirmed: true);

        Cleanup stuff = () =>
        {
            eventContext.Dispose();
            eventContext = null;
        };

        It should_raise_AccountUpdated_event = () =>
            eventContext.ShouldContainEvent<AccountUpdated>();

        It should_not_raise_AccountConfirmed_event = () =>
            eventContext.ShouldNotContainEvent<AccountConfirmed>();

        private static EventContext eventContext;
        private static AccountAR account;
    }
}