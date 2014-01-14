using System;
using Machine.Specifications;
using Ncqrs.Spec;
using WB.Core.BoundedContexts.Designer.Aggregates;
using WB.UI.Designer.Providers.CQRS.Accounts.Events;

namespace WB.Core.BoundedContexts.Designer.Tests.AccountTests
{
    internal class when_updating_confirmed_account_and_it_should_not_be_confirmed_again : AccountTestsContext
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