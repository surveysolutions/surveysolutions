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
using WB.Core.GenericSubdomains.Portable;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.SharedKernels.DataCollection.Events.User;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates;
using WB.Core.SharedKernels.DataCollection.Views;

namespace WB.Tests.Unit.SharedKernels.DataCollection
{
    [TestFixture]
    internal class UserARTests
    {
        [Test]
        public void Lock_When_called_Then_raised_UserLocked_event()
        {
            // arrange
            User user = CreateUserAR();

            // act
            user.Lock();

            // assert
            Assert.That(GetUser(user).IsLockedByHQ, Is.EqualTo(true));
        }

        [Test]
        public void Lock_When_called_Then_raised_UserLockedBySupervisor_event()
        {
            // arrange
            User user = CreateUserAR();

            // act
            user.LockBySupervisor();

            // assert
            Assert.That(this.GetUser(user).IsLockedBySupervisor, Is.EqualTo(true));
        }


        [Test]
        public void Unlock_When_called_Then_raised_UserUnlocked_event()
        {
            // arrange
            User user = CreateUserAR();

            // act
            user.Unlock();

            // assert
            Assert.That(GetUser(user).IsLockedByHQ, Is.EqualTo(false));
        }

        [Test]
        public void Unlock_When_called_Then_raised_UserUnlockedBySupervisor_event()
        {
            // arrange
            User user = CreateUserAR();

            // act
            user.UnlockBySupervisor();

            // assert
            Assert.That(this.GetUser(user).IsLockedBySupervisor, Is.EqualTo(false));
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
            Assert.That(this.GetUser(user).IsLockedBySupervisor, Is.EqualTo(true));
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
            Assert.That(this.GetUser(user).IsLockedByHQ, Is.EqualTo(true));
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
            Assert.That(this.GetUser(user).Email, Is.EqualTo(specifiedEmail));
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
                "my@email.com", isLockedBySupervisor, isLockedByHQ, "pwd", user.Id,
                new UserRoles[] { }, null, "name", string.Empty, string.Empty);

            // assert
            Assert.That(this.GetUser(user).IsLockedBySupervisor, Is.EqualTo(true));
        }

        [Test]
        public void ctor_When_name_is_specified_Then_raised_NewUserCreated_event_with_specified_name()
        {
            // arrange
            string specifiedName = "Green Lantern";
            User user = CreateUserAR();

            // act
            user.CreateUser(
                "my@email.com", false, false, "pwd", user.Id,
                new UserRoles[] { }, null, specifiedName, string.Empty, string.Empty);

            // assert
            Assert.That(this.GetUser(user).UserName, Is.EqualTo(specifiedName));
        }

        [Test]
        public void ctor_When_password_is_specified_Then_raised_NewUserCreated_event_with_specified_password()
        {
            // arrange
            string specifiedPassword = "hhg<8923s:0";
            User user = CreateUserAR();

            // act
            user.CreateUser(
                "my@email.com", false, false, specifiedPassword, user.Id,
                new UserRoles[] { }, null, "name", string.Empty, string.Empty);

            // assert
            Assert.That(this.GetUser(user).Password, Is.EqualTo(specifiedPassword));
        }

        [Test]
        public void ctor_When_email_is_specified_Then_raised_NewUserCreated_event_with_specified_email()
        {
            // arrange
            string specifiedEmail = "gmail@chucknorris.com";
            User user = CreateUserAR();

            // act
            user.CreateUser(
                specifiedEmail, false, false, "pwd", user.Id,
                new UserRoles[] { }, null, "name", string.Empty, string.Empty);

            // assert
            Assert.That(this.GetUser(user).Email, Is.EqualTo(specifiedEmail));
        }

        [Test]
        public void ctor_When_public_key_is_specified_Then_raised_NewUserCreated_event_with_specified_public_key()
        {
            // arrange
            Guid specifiedPublicKey = Guid.NewGuid();
            User user = CreateUserAR(specifiedPublicKey);

            // act
            user.CreateUser(
                "my@email.com", false, false, "pwd", specifiedPublicKey,
                new UserRoles[] { }, null, "name", string.Empty, string.Empty);

            // assert
            Assert.That(this.GetUser(user).PublicKey, Is.EqualTo(specifiedPublicKey));
        }

        [Test]
        public void ctor_When_two_roles_are_specified_Then_raised_NewUserCreated_event_with_specified_roles()
        {
            // arrange
            UserRoles[] twoSpecifedRoles = { UserRoles.Supervisor, UserRoles.Observer };
            User user = CreateUserAR();

            // act
            user.CreateUser(
                "my@email.com", false, false, "pwd", user.Id,
                twoSpecifedRoles, null, "name", string.Empty, string.Empty);

            // assert
            Assert.That(this.GetUser(user).Roles, Is.EquivalentTo(twoSpecifedRoles));
        }

        private static User CreateUserAR(Guid? id=null)
        {
            var user = new User();
            user.SetId(id??Guid.NewGuid());
            user.CreateUser("",false, false,"",user.Id,new [] {UserRoles.Interviewer },null,"","","");
            return user;
        }

        private UserDocument GetUser(User user)
        {
            var userDocument = new UserDocument();

            userDocument.UserId = user.Id.FormatGuid();
            userDocument.UserName = user.UserName;
            userDocument.Password = user.Password;
            userDocument.PublicKey = user.Id;
            userDocument.CreationDate = user.CreationDate;
            userDocument.Email = user.Email;
            userDocument.IsLockedBySupervisor = user.IsLockedBySupervisor;
            userDocument.IsLockedByHQ = user.IsLockedByHQ;
            userDocument.Supervisor = user.Supervisor;
            userDocument.PersonName = user.PersonName;
            userDocument.PhoneNumber = user.PhoneNumber;
            userDocument.DeviceId = user.DeviceId;
            userDocument.IsArchived = user.IsArchived;
            userDocument.LastChangeDate = user.LastChangeDate;
            userDocument.Roles = user.Roles.ToHashSet();
            userDocument.DeviceChangingHistory=user.DeviceChangingHistory.ToHashSet();
            return userDocument;
        }
    }
}