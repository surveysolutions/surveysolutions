using Machine.Specifications;
using WB.Core.BoundedContexts.Designer.Aggregates;

namespace WB.Tests.Unit.Designer.BoundedContexts.Designer.UserTests
{
    internal class when_updating_user : UserTestsContext
    {
        Establish context = () =>
        {
            userName = "user name";
            userEmail = "user@e.mail";
            comment = "some comment";
            passwordQuestion = "secret question";

            user = CreateUser();
        };

        Because of = () =>
            user.Update(userName: userName, comment: comment, email: userEmail, passwordQuestion: passwordQuestion, isLockedOut: false,
                isConfirmed: false);

        It should_set_UserName_equal_to_userName = () =>
                user.UserName.ShouldEqual(userName);

        It should_set_Email_equal_to_userEmail = () =>
                user.Email.ShouldEqual(userEmail);

        It should_set_Comment_equal_to_comment = () =>
                user.Comment.ShouldEqual(comment);
        
        It should_set_PasswordQuestion_equal_to_passwordQuestion = () =>
                user.PasswordQuestion.ShouldEqual(passwordQuestion);

        private static User user;
        private static string userName;
        private static string userEmail;
        private static string comment;
        private static string passwordQuestion;
    }
}