using System;
using System.Linq;
using Main.Core.Entities.SubEntities;
using Microsoft.Practices.ServiceLocation;
using Ncqrs.Domain;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.Infrastructure.Aggregates;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.SharedKernels.DataCollection.Commands.User;
using WB.Core.SharedKernels.DataCollection.Exceptions;
using WB.Core.SharedKernels.DataCollection.Views;

namespace WB.Core.SharedKernels.DataCollection.Implementation.Aggregates
{
    public class User : IPlainAggregateRoot
    {
        private IPlainStorageAccessor<UserDocument> userDocumentStorage => ServiceLocator.Current.GetInstance<IPlainStorageAccessor<UserDocument>>();

        private readonly UserRoles[] userRolesWhichAllowToBeDeleted = new[] { UserRoles.Operator, UserRoles.Supervisor };

        public Guid Id { get; private set; }

        public User(){}


        public void SetId(Guid id)
        {
            Id = id;
        }
        public void CreateUser(string email, bool isLockedBySupervisor, bool isLockedByHq, string password, Guid publicKey, UserRoles[] roles, UserLight supervisor, string userName, string personName,
            string phoneNumber)
        {
            //// Check for uniqueness of person name and email!
            var doc = new UserDocument
            {
                UserId = publicKey.FormatGuid(),
                UserName = userName,
                Password = password,
                PublicKey = publicKey,
                CreationDate = DateTime.UtcNow,
                Email = email,
                IsLockedBySupervisor = isLockedBySupervisor,
                IsLockedByHQ = isLockedByHq,
                Supervisor = supervisor,
                Roles = roles.ToHashSet(),
                PersonName = personName,
                PhoneNumber = phoneNumber
            };
            this.userDocumentStorage.Store(doc, doc.UserId);
        }

        public void ChangeUser(string email, bool? isLockedBySupervisor, bool isLockedByHQ, string passwordHash,
            string personName, string phoneNumber, Guid userId)
        {
            UserDocument user = this.userDocumentStorage.GetById(Id.FormatGuid());

            ThrowIfUserArchived(user);

            user.Email = email;
            user.Password = passwordHash;
            user.PersonName = personName;
            user.PhoneNumber = phoneNumber;

            if (isLockedBySupervisor.HasValue)
            {
                user.IsLockedBySupervisor = isLockedBySupervisor.Value;
            }

            user.IsLockedByHQ = isLockedByHQ;

            this.userDocumentStorage.Store(user, this.Id.FormatGuid());
        }

        public void LinkUserToDevice(LinkUserToDevice command)
        {
            UserDocument user = this.userDocumentStorage.GetById(command.Id.FormatGuid());
            ThrowIfUserArchived(user);
            user.DeviceId = command.DeviceId;
            user.DeviceChangingHistory.Add(
                new DeviceInfo { Date = DateTime.UtcNow, DeviceId = command.DeviceId });
            this.userDocumentStorage.Store(user, this.Id.FormatGuid());
        }

        public void Lock()
        {
            UserDocument user = this.userDocumentStorage.GetById(this.Id.FormatGuid());
            ThrowIfUserArchived(user);

            user.IsLockedByHQ = true;

            this.userDocumentStorage.Store(user, this.Id.FormatGuid());
        }

        public void Unlock()
        {
            UserDocument user = this.userDocumentStorage.GetById(this.Id.FormatGuid());
            ThrowIfUserArchived(user);

            user.IsLockedByHQ = false;

            this.userDocumentStorage.Store(user, this.Id.FormatGuid());
        }

        public void LockBySupervisor()
        {
            UserDocument user = this.userDocumentStorage.GetById(this.Id.FormatGuid());
            ThrowIfUserArchived(user);

            user.IsLockedBySupervisor = true;

            this.userDocumentStorage.Store(user, this.Id.FormatGuid());
        }

        public void UnlockBySupervisor()
        {
            UserDocument user = this.userDocumentStorage.GetById(this.Id.FormatGuid());
            ThrowIfUserArchived(user);

            user.IsLockedBySupervisor = false;

            this.userDocumentStorage.Store(user, this.Id.FormatGuid());
        }

        public void Archive()
        {
            UserDocument user = this.userDocumentStorage.GetById(this.Id.FormatGuid());

            ThrowIfUserArchived(user);

            if (user.Roles.Except(userRolesWhichAllowToBeDeleted).Any())
                throw new UserException(
                    $"user in roles {string.Join(",", user.Roles)} can't be deleted",
                    UserDomainExceptionType.RoleDoesntSupportDelete);
            user.IsArchived = true;
            this.userDocumentStorage.Store(user, this.Id.FormatGuid());
        }

        public void Unarchive()
        {
            UserDocument user = this.userDocumentStorage.GetById(this.Id.FormatGuid());
            ThrowIfUserIsNotArchived(user);
            user.IsArchived = false;
            this.userDocumentStorage.Store(user, this.Id.FormatGuid());
        }

        public void UnarchiveUserAndUpdate(string passwordHash, string email, string personName, string phoneNumber)
        {
            UserDocument user = this.userDocumentStorage.GetById(this.Id.FormatGuid());
            ThrowIfUserIsNotArchived(user);

            user.IsArchived = false;
            user.Password = passwordHash;
            user.Email = email;
            user.IsLockedBySupervisor = false;
            user.IsLockedByHQ = false;
            user.PersonName = personName;
            user.PhoneNumber = phoneNumber;

            this.userDocumentStorage.Store(user, this.Id.FormatGuid());
        }

        private void ThrowIfUserIsNotArchived(UserDocument user)
        {
            if (!user.IsArchived)
                throw new UserException("You can't unarchive active user", UserDomainExceptionType.UserIsNotArchived);
        }

        private void ThrowIfUserArchived(UserDocument user)
        {
            if (user.IsArchived)
                throw new UserException("User already archived", UserDomainExceptionType.UserArchived);
        }
    }
}