using System;
using System.Linq;
using Main.Core.Entities.SubEntities;
using Main.Core.Events.User;
using Ncqrs.Domain;
using Ncqrs.Eventing.Sourcing.Snapshotting;
using WB.Core.SharedKernels.DataCollection.Commands.User;
using WB.Core.SharedKernels.DataCollection.Events.User;
using WB.Core.SharedKernels.DataCollection.Exceptions;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates.Snapshots;

namespace WB.Core.SharedKernels.DataCollection.Implementation.Aggregates
{
    public class User : AggregateRootMappedByConvention, ISnapshotable<UserState>
    {
        private bool isUserLockedBySupervisor;
        private bool isUserLockedByHQ;
        private bool isUserArchived;
        private UserRoles[] userRoles = new UserRoles[0];
        private Guid userSupervisorId;
        private string userSupervisorName;
        private string loginName;

        private readonly UserRoles[] userRolesWhichAllowToBeDeleted = new[] {UserRoles.Operator, UserRoles.Supervisor};
        
        public User(){}

        public void CreateUser(string email, bool isLockedBySupervisor, bool isLockedByHq, string password, Guid publicKey, UserRoles[] roles, UserLight supervisor, string userName, string personName,
            string phoneNumber)
        {
            //// Check for uniqueness of person name and email!
            this.ApplyEvent(
                new NewUserCreated
                {
                    Name = userName,
                    Password = password,
                    Email = email,
                    IsLockedBySupervisor = isLockedBySupervisor,
                    IsLocked = isLockedByHq,
                    Roles = roles,
                    Supervisor = supervisor,
                    PersonName = personName,
                    PhoneNumber = phoneNumber,
                    PublicKey = publicKey
                });
        }

        public void ChangeUser(string email, bool? isLockedBySupervisor, bool isLockedByHQ, string passwordHash, 
            string personName, string phoneNumber, Guid userId)
        {
            ThrowIfUserArchived();
            this.ApplyEvent(new UserChanged { Email = email, PasswordHash = passwordHash, PersonName = personName, PhoneNumber = phoneNumber});

            if (isLockedBySupervisor.HasValue && isLockedBySupervisor.Value && !this.isUserLockedBySupervisor)
            {
                this.ApplyEvent(new UserLockedBySupervisor());
            }
            else if (isLockedBySupervisor.HasValue && !isLockedBySupervisor.Value && this.isUserLockedBySupervisor)
            {
                this.ApplyEvent(new UserUnlockedBySupervisor());
            }

            if (isLockedByHQ && !this.isUserLockedByHQ)
            {
                this.ApplyEvent(new UserLocked());
            }
            else if (!isLockedByHQ && this.isUserLockedByHQ)
            {
                this.ApplyEvent(new UserUnlocked());
            }
        }

        public void LinkUserToDevice(LinkUserToDevice command)
        {
            ThrowIfUserArchived();
            this.ApplyEvent(new UserLinkedToDevice
                            {
                                DeviceId = command.DeviceId
                            });
        }

        public void Lock()
        {
            ThrowIfUserArchived();
            this.ApplyEvent(new UserLocked());
        }

        public void Unlock()
        {
            ThrowIfUserArchived();
            this.ApplyEvent(new UserUnlocked());
        }

        public void LockBySupervisor()
        {
            ThrowIfUserArchived();
            this.ApplyEvent(new UserLockedBySupervisor());
        }

        public void UnlockBySupervisor()
        {
            ThrowIfUserArchived();
            this.ApplyEvent(new UserUnlockedBySupervisor());
        }

        public void Archive()
        {
            ThrowIfUserArchived();

            if (userRoles.Except(userRolesWhichAllowToBeDeleted).Any())
                throw new UserException(
                    String.Format("user in roles {0} can't be deleted", string.Join(",", userRoles)),
                    UserDomainExceptionType.RoleDoesntSupportDelete);

            this.ApplyEvent(new UserArchived());
        }

        public void Unarchive()
        {
            ThrowIfUserIsNotArchived();

            this.ApplyEvent(new UserUnarchived());
        }

        public void UnarchiveUserAndUpdate(string passwordHash, string email, string personName, string phoneNumber)
        {
            ThrowIfUserIsNotArchived();

            this.ApplyEvent(new UserUnarchived());
            this.ApplyEvent(
                new NewUserCreated
                {
                    Name = loginName,
                    Password = passwordHash,
                    Email = email,
                    IsLockedBySupervisor = false,
                    IsLocked = false,
                    Roles = userRoles,
                    Supervisor = new UserLight(userSupervisorId, userSupervisorName),
                    PersonName = personName,
                    PhoneNumber = phoneNumber,
                    PublicKey = EventSourceId
                });
        }

        private void ThrowIfUserIsNotArchived()
        {
            if (!isUserArchived)
                throw new UserException("You can't unarchive active user", UserDomainExceptionType.UserIsNotArchived);
        }
        private void ThrowIfUserArchived()
        {
            if (isUserArchived)
                throw new UserException("User already archived", UserDomainExceptionType.UserArchived);
        }

        protected void OnUserUnarchived(UserUnarchived @event)
        {
            isUserArchived = false;
        }

        protected void OnUserArchived(UserArchived @event)
        {
            isUserArchived = true;
        }

        protected void Apply(UserLinkedToDevice @event)
        {
        }

        protected void OnNewUserCreated(NewUserCreated e)
        {
            this.isUserLockedBySupervisor = e.IsLockedBySupervisor;
            this.isUserLockedByHQ = e.IsLocked;
            this.userRoles = e.Roles;
            this.loginName = e.Name;

            if (e.Supervisor != null)
            {
                this.userSupervisorId = e.Supervisor.Id;
                this.userSupervisorName = e.Supervisor.Name;
            }
        }

        protected void OnUserLocked(UserLockedBySupervisor @event)
        {
            this.isUserLockedBySupervisor = true;
        }

        protected void OnUserUnlocked(UserUnlockedBySupervisor @event)
        {
            this.isUserLockedBySupervisor = false;
        }

        protected void OnUserLocked(UserLocked @event)
        {
            this.isUserLockedByHQ = true;
        }

        protected void OnUserUnlocked(UserUnlocked @event)
        {
            this.isUserLockedByHQ = false;
        }

        protected void OnUserChange(UserChanged e)
        {
        }


        public UserState CreateSnapshot()
        {
            return new UserState()
            {
                IsUserArchived = isUserArchived,
                IsUserLockedByHQ = isUserLockedByHQ,
                IsUserLockedBySupervisor = isUserLockedBySupervisor,
                LoginName = loginName,
                UserRoles = userRoles,
                UserSupervisorId = userSupervisorId,
                UserSupervisorName = userSupervisorName
            };
        }

        public void RestoreFromSnapshot(UserState snapshot)
        {
            isUserArchived = snapshot.IsUserArchived;
            isUserLockedByHQ = snapshot.IsUserLockedByHQ;
            isUserLockedBySupervisor = snapshot.IsUserLockedBySupervisor;
            loginName = snapshot.LoginName;
            userRoles = snapshot.UserRoles;
            userSupervisorId = snapshot.UserSupervisorId;
            userSupervisorName = snapshot.UserSupervisorName;
        }
    }
}