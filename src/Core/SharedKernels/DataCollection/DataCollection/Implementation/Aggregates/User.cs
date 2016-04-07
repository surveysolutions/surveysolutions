using System;
using System.Collections.Generic;
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
        private readonly UserRoles[] userRolesWhichAllowToBeDeleted = new[] {UserRoles.Operator, UserRoles.Supervisor};

        public Guid Id { get; private set; }

        public User()
        {
        }


        public void SetId(Guid id)
        {
            Id = id;
        }

        public DateTime CreationDate;
        public string Email;
        public bool IsLockedByHQ;
        public bool IsArchived;
        public bool IsLockedBySupervisor;
        public string Password;
        public UserLight Supervisor;
        public string UserName;
        public DateTime LastChangeDate;
        public string DeviceId;
        public UserRoles[] Roles=new UserRoles[0];
        public List<DeviceInfo> DeviceChangingHistory =new List<DeviceInfo>();
        public string PersonName;
        public string PhoneNumber;

        public void CreateUser(string email, bool isLockedBySupervisor, bool isLockedByHq, string password,
            Guid publicKey, UserRoles[] roles, UserLight supervisor, string userName, string personName,
            string phoneNumber)
        {
            //// Check for uniqueness of person name and email!

            UserName = userName;
            Password = password;
            CreationDate = DateTime.UtcNow;
            Email = email;
            IsLockedBySupervisor = isLockedBySupervisor;
            IsLockedByHQ = isLockedByHq;
            Supervisor = supervisor;
            Roles = roles;
            PersonName = personName;
            PhoneNumber = phoneNumber;
        }

        public void ChangeUser(string email, bool? isLockedBySupervisor, bool isLockedByHQ, string passwordHash,
            string personName, string phoneNumber, Guid userId)
        {
            ThrowIfUserArchived();

            Email = email;
            Password = passwordHash;
            PersonName = personName;
            PhoneNumber = phoneNumber;

            if (isLockedBySupervisor.HasValue)
            {
                IsLockedBySupervisor = isLockedBySupervisor.Value;
            }

            IsLockedByHQ = isLockedByHQ;
        }

        public void LinkUserToDevice(LinkUserToDevice command)
        {
            ThrowIfUserArchived();
            DeviceId = command.DeviceId;
            DeviceChangingHistory.Add(
                new DeviceInfo {Date = DateTime.UtcNow, DeviceId = command.DeviceId});
        }

        public void Lock()
        {
            ThrowIfUserArchived();

            IsLockedByHQ = true;
        }

        public void Unlock()
        {
            ThrowIfUserArchived();

            IsLockedByHQ = false;
        }

        public void LockBySupervisor()
        {
            ThrowIfUserArchived();

            IsLockedBySupervisor = true;
        }

        public void UnlockBySupervisor()
        {
            ThrowIfUserArchived();

            IsLockedBySupervisor = false;
        }

        public void Archive()
        {
            ThrowIfUserArchived();

            if (Roles.Except(userRolesWhichAllowToBeDeleted).Any())
                throw new UserException(
                    $"user in roles {string.Join(",", Roles)} can't be deleted",
                    UserDomainExceptionType.RoleDoesntSupportDelete);
            IsArchived = true;
        }

        public void Unarchive()
        {
            ThrowIfUserIsNotArchived();
            IsArchived = false;
        }

        public void UnarchiveUserAndUpdate(string passwordHash, string email, string personName, string phoneNumber)
        {
            ThrowIfUserIsNotArchived();

            IsArchived = false;
            Password = passwordHash;
            Email = email;
            IsLockedBySupervisor = false;
            IsLockedByHQ = false;
            PersonName = personName;
            PhoneNumber = phoneNumber;
        }

        private void ThrowIfUserIsNotArchived()
        {
            if (!IsArchived)
                throw new UserException("You can't unarchive active user", UserDomainExceptionType.UserIsNotArchived);
        }

        private void ThrowIfUserArchived()
        {
            if (IsArchived)
                throw new UserException("User already archived", UserDomainExceptionType.UserArchived);
        }
    }
}