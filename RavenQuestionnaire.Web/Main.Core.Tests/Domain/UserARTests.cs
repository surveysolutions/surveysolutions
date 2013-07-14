using Microsoft.Practices.ServiceLocation;
using Moq;

namespace Main.Core.Tests.Domain
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;

    using Main.Core.Domain;
    using Main.Core.Entities.SubEntities;
    using Main.Core.Events.User;

    using NUnit.Framework;

    using Ncqrs.Spec;

    [TestFixture]
    public class UserARTests
    {
        private EventContext eventContext;

        [SetUp]
        public void Init()
        {
	        ServiceLocator.SetLocatorProvider(() => new Mock<IServiceLocator> { DefaultValue = DefaultValue.Mock }.Object);
            this.eventContext = new EventContext();
        }

        [TearDown]
        public void Dispose()
        {
            this.eventContext.Dispose();
            this.eventContext = null;
        }

        [Test]
        public void Lock_When_called_Then_raised_UserLocked_event()
        {
            // arrange
            UserAR user = CreateUserAR();

            // act
            user.Lock();

            // assert
            Assert.That(this.GetRaisedEvents<UserLocked>().Count(), Is.EqualTo(1));
        }

        [Test]
        public void Unlock_When_called_Then_raised_UserUnlocked_event()
        {
            // arrange
            UserAR user = CreateUserAR();

            // act
            user.Unlock();

            // assert
            Assert.That(this.GetRaisedEvents<UserUnlocked>().Count(), Is.EqualTo(1));
        }

        [Test]
        public void ChangeUser_When_is_locked_set_to_true_Then_raised_UserLocked_event()
        {
            // arrange
            UserAR user = CreateUserAR();
            bool isLocked = true;

            // act
            user.ChangeUser("mail@domain.net", isLocked, new UserRoles[] { });

            // assert
            Assert.That(this.GetRaisedEvents<UserLocked>().Count(), Is.EqualTo(1));
        }

        [Test]
        public void ChangeUser_When_is_locked_set_to_false_Then_raised_UserUnlocked_event()
        {
            // arrange
            UserAR user = CreateUserAR();
            bool isLocked = false;

            // act
            user.ChangeUser("mail@domain.net", isLocked, new UserRoles[] { });

            // assert
            Assert.That(this.GetRaisedEvents<UserUnlocked>().Count(), Is.EqualTo(1));
        }

        [Test]
        public void ChangeUser_When_email_is_specified_Then_raised_UserChanged_event_with_specified_email()
        {
            // arrange
            UserAR user = CreateUserAR();
            string specifiedEmail = "user@example.com";

            // act
            user.ChangeUser(specifiedEmail, false, new UserRoles[] { });

            // assert
            Assert.That(this.GetSingleRaisedEvent<UserChanged>().Email, Is.EqualTo(specifiedEmail));
        }

        [Test]
        public void ChangeUser_When_two_roles_are_specified_Then_raised_UserChanged_event_with_specified_roles()
        {
            // arrange
            UserAR user = CreateUserAR();
            IEnumerable<UserRoles> twoSpecifedRoles = new [] { UserRoles.Administrator, UserRoles.User };

            // act
            user.ChangeUser("mail@domain.net", false, twoSpecifedRoles.ToArray());

            // assert
            Assert.That(this.GetSingleRaisedEvent<UserChanged>().Roles, Is.EquivalentTo(twoSpecifedRoles));
        }

        [Test]
        public void ctor_When_is_locked_set_to_true_Then_raised_NewUserCreated_event_with_is_locked_set_to_true()
        {
            // arrange
            bool isLocked = true;

            // act
            new UserAR(Guid.NewGuid(), "name", "pwd", "my@email.com", new UserRoles[] { }, isLocked, null);

            // assert
            Assert.That(this.GetSingleRaisedEvent<NewUserCreated>().IsLocked, Is.EqualTo(true));
        }

        [Test]
        public void ctor_When_name_is_specified_Then_raised_NewUserCreated_event_with_specified_name()
        {
            // arrange
            string specifiedName = "Green Lantern";

            // act
            new UserAR(Guid.NewGuid(), specifiedName, "pwd", "my@email.com", new UserRoles[] { }, false, null);

            // assert
            Assert.That(this.GetSingleRaisedEvent<NewUserCreated>().Name, Is.EqualTo(specifiedName));
        }

        [Test]
        public void ctor_When_password_is_specified_Then_raised_NewUserCreated_event_with_specified_password()
        {
            // arrange
            string specifiedPassword = "hhg<8923s:0";

            // act
            new UserAR(Guid.NewGuid(), "name", specifiedPassword, "my@email.com", new UserRoles[] { }, false, null);

            // assert
            Assert.That(this.GetSingleRaisedEvent<NewUserCreated>().Password, Is.EqualTo(specifiedPassword));
        }

        [Test]
        public void ctor_When_email_is_specified_Then_raised_NewUserCreated_event_with_specified_email()
        {
            // arrange
            string specifiedEmail = "gmail@chucknorris.com";

            // act
            new UserAR(Guid.NewGuid(), "name", "pwd", specifiedEmail, new UserRoles[] { }, false, null);

            // assert
            Assert.That(this.GetSingleRaisedEvent<NewUserCreated>().Email, Is.EqualTo(specifiedEmail));
        }

        [Test]
        public void ctor_When_public_key_is_specified_Then_raised_NewUserCreated_event_with_specified_public_key()
        {
            // arrange
            Guid specifiedPublicKey = Guid.NewGuid();

            // act
            new UserAR(specifiedPublicKey, "name", "pwd", "my@email.com", new UserRoles[] { }, false, null);

            // assert
            Assert.That(this.GetSingleRaisedEvent<NewUserCreated>().PublicKey, Is.EqualTo(specifiedPublicKey));
        }

        [Test]
        public void ctor_When_three_roles_are_specified_Then_raised_NewUserCreated_event_with_specified_roles()
        {
            // arrange
            IEnumerable<UserRoles> threeSpecifedRoles = new [] { UserRoles.Supervisor, UserRoles.Operator, UserRoles.User };

            // act
            new UserAR(Guid.NewGuid(), "name", "pwd", "my@email.com", threeSpecifedRoles.ToArray(), false, null);

            // assert
            Assert.That(this.GetSingleRaisedEvent<NewUserCreated>().Roles, Is.EquivalentTo(threeSpecifedRoles));
        }

        private static UserAR CreateUserAR()
        {
            return new UserAR();
        }

        private T GetSingleRaisedEvent<T>()
        {
            return this.GetRaisedEvents<T>().Single();
        }

        private IEnumerable<T> GetRaisedEvents<T>()
        {
            return this.eventContext
                .Events
                .Where(e => e.Payload is T)
                .Select(e => e.Payload)
                .Cast<T>();
        }
    }
}