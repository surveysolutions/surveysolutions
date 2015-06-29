using System;
using System.Linq;
using Main.Core.Entities.SubEntities;
using Main.Core.Events.User;
using Microsoft.Practices.ServiceLocation;
using Ncqrs.Domain;
using WB.Core.SharedKernels.DataCollection.Commands.User;
using WB.Core.SharedKernels.DataCollection.Events.User;
using WB.Core.SharedKernels.DataCollection.Exceptions;
using WB.Core.SharedKernels.DataCollection.Services;

namespace WB.Core.SharedKernels.DataCollection.Implementation.Aggregates
{
    public class User : AggregateRootMappedByConvention
    {
        private bool isUserLockedBySupervisor;
        private bool isUserLockedByHQ;
        private bool isUserArchived;
        private UserRoles[] roles = new UserRoles[0];

        public User(){}

        public User(Guid publicKey, string userName, string password, string email, UserRoles[] roles, bool isLockedbySupervisor,
            bool isLockedbyHQ, UserLight supervisor, string personName, string phoneNumber)
            : base(publicKey)
        {
            this.CreateUser(email, isLockedbySupervisor, isLockedbyHQ, password, publicKey, roles, supervisor, userName, personName, phoneNumber);
        }

        private IUserPreconditionsService UserPreconditionsService
        {
            get { return ServiceLocator.Current.GetInstance<IUserPreconditionsService>(); }
        }

        public void CreateUser(string email, bool isLockedBySupervisor, bool isLockedByHq, string password, Guid publicKey, UserRoles[] roles, UserLight supervisor, string userName, string personName,
            string phoneNumber)
        {
            if (UserPreconditionsService.IsUserNameTakenByActiveUsers(userName))
                throw new UserException(String.Format("user name '{0}' is taken", userName), UserDomainExceptionType.UserNameTakenByActiveUsers);

            if (UserPreconditionsService.IsUserNameTakenByArchivedUsers(userName))
                throw new UserException(String.Format("user name '{0}' is taken by archived users", userName), UserDomainExceptionType.UserNameTakenByArchivedUsers);

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
            if (roles.Contains(UserRoles.Operator))
            {
                var countOfInterviewsUserResposibleFor =
                    UserPreconditionsService.CountOfInterviewsInterviewerResposibleFor(EventSourceId);

                if (countOfInterviewsUserResposibleFor > 0)
                {
                    throw new UserException(String.Format(
                        "Interviewer {0} is resposible for {1} interview(s) and can't be deleted", EventSourceId,
                        countOfInterviewsUserResposibleFor), UserDomainExceptionType.UserHasAssigments);
                }
            }
            else if (roles.Contains(UserRoles.Supervisor))
            {
                var countOfActiveInterviewersForSupervisor = UserPreconditionsService.CountOfActiveInterviewersForSupervisor(EventSourceId);
                if (countOfActiveInterviewersForSupervisor > 0)
                {
                    throw new UserException(String.Format(
                        "Supervisor {0} has {1} active interrviewer(s) and can't be deleted", EventSourceId,
                        countOfActiveInterviewersForSupervisor), UserDomainExceptionType.UserHasAssigments);
                }
            }
            else
            {
                throw new UserException(String.Format("user in roles {0} can't be deleted", string.Join(",", roles)), UserDomainExceptionType.RoleDoesntSupportDelete);
            }
            this.ApplyEvent(new UserArchived());
        }

        public void Unarchive()
        {
            if (!isUserArchived)
                throw new UserException("You can't unarchive active user", UserDomainExceptionType.UserIsNotArchived);

            this.ApplyEvent(new UserUnarchived());
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
            this.roles = e.Roles;
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

       
    }
}