using NUnit.Framework;
using WB.Core.BoundedContexts.Designer.Aggregates;

namespace WB.Tests.Unit.Designer.BoundedContexts.Designer.UserTests
{
    [TestOf(typeof(User))]
    internal class UserTests : UserTestsContext
    {
        [Test]
        public void when_updating_user()
        {
            // arrange
            var userName = "user name";
            var userEmail = "user@e.mail";
            var comment = "some comment";
            var passwordQuestion = "secret question";
            var fullName = "John Doe";

            var user = CreateUser();
            // act
            user.Update(userName: userName, comment: comment, email: userEmail, passwordQuestion: passwordQuestion, isLockedOut: false,
                isConfirmed: false, canImportOnHq: false, fullName: fullName);

            // assert
            Assert.That(user.UserName, Is.EqualTo(userName));
            Assert.That(user.Email, Is.EqualTo(userEmail));
            Assert.That(user.Comment, Is.EqualTo(comment));
            Assert.That(user.PasswordQuestion, Is.EqualTo(passwordQuestion));
            Assert.That(user.FullName, Is.EqualTo(fullName));
        }

    }
}
