using System;
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

        [Test]
        public void when_register_user_with_full_name()
        {
            // arrange
            string applicationName = "app";
            string userName = "name";
            string email = "aaa@aa.aaa";
            Guid accountId = Guid.NewGuid();
            string password = "P@$$w0rd";
            string passwordSalt = "salt";
            bool isConfirmed = true;
            string confirmationToken = "token";
            string fullName = "John Doe";

            var user = CreateUser();
            // act
            user.Register(applicationName, userName, email, accountId, password, passwordSalt, isConfirmed, confirmationToken, fullName);

            // assert
            Assert.That(user.ApplicationName, Is.EqualTo(applicationName));
            Assert.That(user.UserName, Is.EqualTo(userName));
            Assert.That(user.Email, Is.EqualTo(email));
            Assert.That(user.Password, Is.EqualTo(password));
            Assert.That(user.PasswordSalt, Is.EqualTo(passwordSalt));
            Assert.That(user.IsConfirmed, Is.EqualTo(isConfirmed));
            Assert.That(user.ConfirmationToken, Is.EqualTo(confirmationToken));
            Assert.That(user.FullName, Is.EqualTo(fullName));
        }

    }
}
