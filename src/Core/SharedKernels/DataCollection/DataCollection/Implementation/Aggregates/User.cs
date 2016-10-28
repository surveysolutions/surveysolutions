using System;
using System.Collections.Generic;
using System.Linq;
using Main.Core.Entities.SubEntities;
using WB.Core.Infrastructure.Aggregates;
using WB.Core.SharedKernels.DataCollection.Commands.User;
using WB.Core.SharedKernels.DataCollection.Exceptions;
using WB.Core.SharedKernels.DataCollection.Views;

namespace WB.Core.SharedKernels.DataCollection.Implementation.Aggregates
{
    public class User : IPlainAggregateRoot
    {
        private static readonly UserRoles[] RolesWhichSupportArchiving = { UserRoles.Interviewer, UserRoles.Supervisor };

        public Guid Id { get; private set; }
        

        public void SetId(Guid id)
        {
            this.Id = id;
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

        public void LinkUserToDevice(LinkUserToDevice command)
        {
            this.ThrowIfUserArchived();

            this.DeviceId = command.DeviceId;
            this.DeviceChangingHistory.Add(
                new DeviceInfo {Date = DateTime.UtcNow, DeviceId = command.DeviceId});
        }

        public void Archive()
        {
            this.ThrowIfUserArchived();
            this.ThrowIfRoleRestrictsArchiving();

            this.IsArchived = true;
        }

        public void Unarchive()
        {
            this.ThrowIfUserIsNotArchived();
            this.IsArchived = false;
        }

        public void UnarchiveAndUpdate(string passwordHash, string email, string personName, string phoneNumber)
        {
            this.ThrowIfUserIsNotArchived();

            this.IsArchived = false;
            this.Password = passwordHash;
            this.Email = email;
            this.IsLockedBySupervisor = false;
            this.IsLockedByHQ = false;
            this.PersonName = personName;
            this.PhoneNumber = phoneNumber;
        }

        private void ThrowIfUserIsNotArchived()
        {
            if (!this.IsArchived)
                throw new UserException("You can't unarchive active user", UserDomainExceptionType.UserIsNotArchived);
        }

        private void ThrowIfUserArchived()
        {
            if (this.IsArchived)
                throw new UserException("User already archived", UserDomainExceptionType.UserArchived);
        }

        private void ThrowIfRoleRestrictsArchiving()
        {
            if (this.Roles.Except(RolesWhichSupportArchiving).Any())
                throw new UserException(
                    $"user in roles {string.Join(",", this.Roles)} can't be deleted",
                    UserDomainExceptionType.RoleDoesntSupportDelete);
        }
    }
}