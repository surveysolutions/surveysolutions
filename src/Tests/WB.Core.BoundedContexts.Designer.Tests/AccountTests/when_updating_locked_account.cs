using System;
using Machine.Specifications;
using Ncqrs.Spec;
using WB.Core.BoundedContexts.Designer.Aggregates;
using WB.UI.Designer.Providers.CQRS.Accounts.Events;

namespace WB.Core.BoundedContexts.Designer.Tests.AccountTests
{
    internal class when_updating_locked_account : AccountTestsContext
    {
        Establish context = () =>
        {
            accounId = Guid.Parse("11111111111111111111111111111111");

            account = CreateAccount(accounId);
            account.Apply(new AccountLocked());

            eventContext = new EventContext();
        };

        Because of = () =>
            account.Update(userName: null, comment: null, email: null, passwordQuestion: null, isLockedOut: false, isConfirmed: false);

        Cleanup stuff = () =>
        {
            eventContext.Dispose();
            eventContext = null;
        };

        It should_raise_AccountUnlocked = () =>
            eventContext.ShouldContainEvent<AccountUnlocked>();

        private static EventContext eventContext;
        private static AccountAR account;
        private static Guid accounId;
    }
}