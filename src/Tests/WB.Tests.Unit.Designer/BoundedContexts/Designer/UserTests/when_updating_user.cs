using Machine.Specifications;
using WB.Core.BoundedContexts.Designer.Aggregates;

namespace WB.Tests.Unit.Designer.BoundedContexts.Designer.UserTests
{
    internal class when_updating_user : UserTestsContext
    {
        [NUnit.Framework.OneTimeSetUp] public void context () {
            userName = "user name";
            userEmail = "user@e.mail";
            comment = "some comment";
            passwordQuestion = "secret question";

            user = CreateUser();
            BecauseOf();
        }

        private void BecauseOf() =>
            user.Update(userName: userName, comment: comment, email: userEmail, passwordQuestion: passwordQuestion, isLockedOut: false,
                isConfirmed: false, canImportOnHq: false);

        [NUnit.Framework.Test] public void should_set_UserName_equal_to_userName () =>
                user.UserName.ShouldEqual(userName);

        [NUnit.Framework.Test] public void should_set_Email_equal_to_userEmail () =>
                user.Email.ShouldEqual(userEmail);

        [NUnit.Framework.Test] public void should_set_Comment_equal_to_comment () =>
                user.Comment.ShouldEqual(comment);
        
        [NUnit.Framework.Test] public void should_set_PasswordQuestion_equal_to_passwordQuestion () =>
                user.PasswordQuestion.ShouldEqual(passwordQuestion);

        private static User user;
        private static string userName;
        private static string userEmail;
        private static string comment;
        private static string passwordQuestion;
    }
}