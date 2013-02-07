namespace Main.Core.Tests.Domain
{
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