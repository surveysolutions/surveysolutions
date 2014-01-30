﻿using System;
using Machine.Specifications;
using Ncqrs.Spec;
using WB.Core.BoundedContexts.Designer.Aggregates;
using WB.UI.Designer.Providers.CQRS.Accounts.Events;

namespace WB.Core.BoundedContexts.Designer.Tests.AccountTests
{
    internal class when_updating_not_confirmed_account_and_specifying_is_confirmed_true : AccountTestsContext
    {
        Establish context = () =>
        {
            var accountId = Guid.Parse("11111111111111111111111111111111");

            account = CreateAccount(accountId);

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

        It should_raise_AccountConfirmed_event = () =>
            eventContext.ShouldContainEvent<AccountConfirmed>();

        private static EventContext eventContext;
        private static AccountAR account;
    }
}