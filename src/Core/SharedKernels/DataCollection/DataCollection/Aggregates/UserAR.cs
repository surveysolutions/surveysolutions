﻿using System;
using Main.Core.Entities.SubEntities;
using Main.Core.Events.User;
using Ncqrs.Domain;
using WB.Core.SharedKernels.DataCollection.Events.User;


namespace WB.Core.SharedKernels.DataCollection.Aggregates
{
    public class UserAR : AggregateRootMappedByConvention
    {
        private bool isUserLockedBySupervisor;

        private bool isUserLockedByHQ;
        
        public UserAR(){}

        public UserAR(Guid publicKey, string userName, string password, string email, UserRoles[] roles, bool isLockedbySupervisor, 
            bool isLockedbyHQ, UserLight supervisor) : base(publicKey)
        {
            this.CreateUser(email, isLockedbySupervisor, isLockedbyHQ, password, publicKey, roles, supervisor, userName);
        }

        public void CreateUser(string email, bool isLockedBySupervisor, bool isLockedByHq, string password, Guid publicKey, UserRoles[] roles, UserLight supervisor, string userName)
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
                    PublicKey = publicKey
                });
        }

        public void ChangeUser(string email, bool? isLockedBySupervisor, bool isLockedByHQ, UserRoles[] roles, string passwordHash, Guid userId)
        {
            this.ApplyEvent(new UserChanged { Email = email, Roles = roles, PasswordHash = passwordHash});

            if (isLockedBySupervisor.HasValue && isLockedBySupervisor.Value && !isUserLockedBySupervisor)
            {
                this.ApplyEvent(new UserLockedBySupervisor());
            }
            else if (isLockedBySupervisor.HasValue && !isLockedBySupervisor.Value && isUserLockedBySupervisor)
            {
                this.ApplyEvent(new UserUnlockedBySupervisor());
            }

            if (isLockedByHQ && !isUserLockedByHQ)
            {
                this.ApplyEvent(new UserLocked());
            }
            else if (!isLockedByHQ && isUserLockedByHQ)
            {
                this.ApplyEvent(new UserUnlocked());
            }
        }

        public void Lock()
        {
            this.ApplyEvent(new UserLocked());
        }

        public void Unlock()
        {
            this.ApplyEvent(new UserUnlocked());
        }

        public void LockBySupervisor()
        {
            this.ApplyEvent(new UserLockedBySupervisor());
        }

        public void UnlockBySupervisor()
        {
            this.ApplyEvent(new UserUnlockedBySupervisor());
        }

        protected void OnNewUserCreated(NewUserCreated e)
        {
            this.isUserLockedBySupervisor = e.IsLockedBySupervisor;
            this.isUserLockedByHQ = e.IsLocked;
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