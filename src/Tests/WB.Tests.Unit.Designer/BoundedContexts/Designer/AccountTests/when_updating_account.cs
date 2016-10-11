using System;
using Machine.Specifications;
using WB.Core.BoundedContexts.Designer.Aggregates;
using WB.UI.Designer.Providers.CQRS.Accounts.Events;

namespace WB.Tests.Unit.Designer.BoundedContexts.Designer.AccountTests
{
    [Ignore("KP-7922 KP-7923")]
    internal class when_updating_account : AccountTestsContext
    {
        Establish context = () =>
        {
            var accountId = Guid.Parse("11111111111111111111111111111111");
            userName = "user name";
            userEmail = "user@e.mail";
            comment = "some comment";
            passwordQuestion = "secret question";

            account = CreateAccount(accountId);

            eventContext = new EventContext();
        };

        Because of = () =>
            account.Update(userName: userName, comment: comment, email: userEmail, passwordQuestion: passwordQuestion, isLockedOut: false,
                isConfirmed: false);

        Cleanup stuff = () =>
        {
            eventContext.Dispose();
            eventContext = null;
        };

        It should_raise_AccountUpdated_event = () =>
            eventContext.ShouldContainEvent<AccountUpdated>();

        It should_raise_AccountUpdated_event_with_UserName_equal_to_userName = () =>
            eventContext.GetSingleEvent<AccountUpdated>().UserName.ShouldEqual(userName);

        It should_raise_AccountUpdated_event_with_Email_equal_to_userEmail = () =>
            eventContext.GetSingleEvent<AccountUpdated>().Email.ShouldEqual(userEmail);

        It should_raise_AccountUpdated_event_with_Comment_equal_to_comment = () =>
            eventContext.GetSingleEvent<AccountUpdated>().Comment.ShouldEqual(comment);

        It should_raise_AccountUpdated_event_with_PasswordQuestion_equal_to_passwordQuestion = () =>
            eventContext.GetSingleEvent<AccountUpdated>().PasswordQuestion.ShouldEqual(passwordQuestion);

        private static EventContext eventContext;
        private static Account account;
        private static string userName;
        private static string userEmail;
        private static string comment;
        private static string passwordQuestion;
    }
}