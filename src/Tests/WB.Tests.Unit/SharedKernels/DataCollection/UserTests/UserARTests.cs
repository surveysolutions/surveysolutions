using System;
using System.Collections.Generic;
using System.Linq;
using Main.Core.Entities.SubEntities;
using Main.Core.Events.User;
using Microsoft.Practices.ServiceLocation;
using Moq;
using Ncqrs;
using Ncqrs.Spec;
using NUnit.Framework;
using WB.Core.SharedKernels.DataCollection.Events.User;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates;

namespace WB.Tests.Unit.SharedKernels.DataCollection
{
    [TestFixture]
    internal class UserARTests
    {
        private EventContext eventContext;

        [SetUp]
        public void Init()
        {
            AssemblyContext.SetupServiceLocator();

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
            User user = CreateUserAR();

            // act
            user.Lock();

            // assert
            Assert.That(this.GetRaisedEvents<UserLocked>().Count(), Is.EqualTo(1));
        }

        [Test]
        public void Lock_When_called_Then_raised_UserLockedBySupervisor_event()
        {
            // arrange
            User user = CreateUserAR();

            // act
            user.LockBySupervisor();

            // assert
            Assert.That(this.GetRaisedEvents<UserLockedBySupervisor>().Count(), Is.EqualTo(1));
        }


        [Test]
        public void Unlock_When_called_Then_raised_UserUnlocked_event()
        {
            // arrange
            User user = CreateUserAR();

            // act
            user.Unlock();

            // assert
            Assert.That(this.GetRaisedEvents<UserUnlocked>().Count(), Is.EqualTo(1));
        }

        [Test]
        public void Unlock_When_called_Then_raised_UserUnlockedBySupervisor_event()
        {
            // arrange
            User user = CreateUserAR();

            // act
            user.UnlockBySupervisor();

            // assert
            Assert.That(this.GetRaisedEvents<UserUnlockedBySupervisor>().Count(), Is.EqualTo(1));
        }


        [Test]
        public void ChangeUser_When_is_locked_set_to_true_Then_raised_UserLockedBySupervisor_event()
        {
            // arrange
            User user = CreateUserAR();
            bool isLockedBySupervisor = true;
            bool isLockedByHQ = false;

            // act
            user.ChangeUser("mail@domain.net", isLockedBySupervisor, isLockedByHQ, string.Empty, string.Empty, String.Empty, Guid.Empty);

            // assert
            Assert.That(this.GetRaisedEvents<UserLockedBySupervisor>().Count(), Is.EqualTo(1));
        }

        [Test]
        public void ChangeUser_When_is_locked_by_hq_set_to_true_Then_raised_UserLocked_event()
        {
            // arrange
            User user = CreateUserAR();
            bool isLockedBySupervisor = false;
            bool isLockedByHQ = true;

            // act
            user.ChangeUser("mail@domain.net", isLockedBySupervisor, isLockedByHQ, string.Empty, string.Empty, string.Empty, Guid.Empty);

            // assert
            Assert.That(this.GetRaisedEvents<UserLocked>().Count(), Is.EqualTo(1));
        }

        [Test]
        public void ChangeUser_When_email_is_specified_Then_raised_UserChanged_event_with_specified_email()
        {
            // arrange
            User user = CreateUserAR();
            string specifiedEmail = "user@example.com";

            // act
            user.ChangeUser(specifiedEmail, false, false, string.Empty, string.Empty, string.Empty, Guid.Empty);

            // assert
            Assert.That(this.GetSingleRaisedEvent<UserChanged>().Email, Is.EqualTo(specifiedEmail));
        }

        [Test]
        public void ctor_When_is_locked_set_to_true_Then_raised_NewUserCreated_event_with_is_locked_set_to_true()
        {
            // arrange
            bool isLockedBySupervisor = true;
            bool isLockedByHQ = false;
            User user = CreateUserAR();

            // act
            user.CreateUser(
                "my@email.com", isLockedBySupervisor, isLockedByHQ, "pwd", Guid.NewGuid(),
                new UserRoles[] { }, null, "name", string.Empty, string.Empty);

            // assert
            Assert.That(this.GetSingleRaisedEvent<NewUserCreated>().IsLockedBySupervisor, Is.EqualTo(true));
        }

        [Test]
        public void ctor_When_name_is_specified_Then_raised_NewUserCreated_event_with_specified_name()
        {
            // arrange
            string specifiedName = "Green Lantern";
            User user = CreateUserAR();

            // act
            user.CreateUser(
                "my@email.com", false, false, "pwd", Guid.NewGuid(),
                new UserRoles[] { }, null, specifiedName, string.Empty, string.Empty);

            // assert
            Assert.That(this.GetSingleRaisedEvent<NewUserCreated>().Name, Is.EqualTo(specifiedName));
        }

        [Test]
        public void ctor_When_password_is_specified_Then_raised_NewUserCreated_event_with_specified_password()
        {
            // arrange
            string specifiedPassword = "hhg<8923s:0";
            User user = CreateUserAR();

            // act
            user.CreateUser(
                "my@email.com", false, false, specifiedPassword, Guid.NewGuid(),
                new UserRoles[] { }, null, "name", string.Empty, string.Empty);

            // assert
            Assert.That(this.GetSingleRaisedEvent<NewUserCreated>().Password, Is.EqualTo(specifiedPassword));
        }

        [Test]
        public void ctor_When_email_is_specified_Then_raised_NewUserCreated_event_with_specified_email()
        {
            // arrange
            string specifiedEmail = "gmail@chucknorris.com";
            User user = CreateUserAR();

            // act
            user.CreateUser(
                specifiedEmail, false, false, "pwd", Guid.NewGuid(),
                new UserRoles[] { }, null, "name", string.Empty, string.Empty);

            // assert
            Assert.That(this.GetSingleRaisedEvent<NewUserCreated>().Email, Is.EqualTo(specifiedEmail));
        }

        [Test]
        public void ctor_When_public_key_is_specified_Then_raised_NewUserCreated_event_with_specified_public_key()
        {
            // arrange
            Guid specifiedPublicKey = Guid.NewGuid();
            User user = CreateUserAR();

            // act
            user.CreateUser(
                "my@email.com", false, false, "pwd", specifiedPublicKey,
                new UserRoles[] { }, null, "name", string.Empty, string.Empty);

            // assert
            Assert.That(this.GetSingleRaisedEvent<NewUserCreated>().PublicKey, Is.EqualTo(specifiedPublicKey));
        }

        [Test]
        public void ctor_When_two_roles_are_specified_Then_raised_NewUserCreated_event_with_specified_roles()
        {
            // arrange
            UserRoles[] twoSpecifedRoles = { UserRoles.Supervisor, UserRoles.User };
            User user = CreateUserAR();

            // act
            user.CreateUser(
                "my@email.com", false, false, "pwd", Guid.NewGuid(),
                twoSpecifedRoles, null, "name", string.Empty, string.Empty);

            // assert
            Assert.That(this.GetSingleRaisedEvent<NewUserCreated>().Roles, Is.EquivalentTo(twoSpecifedRoles));
        }

        private static User CreateUserAR()
        {
            return new User();
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