using System;
using Main.Core.Entities.SubEntities;
using Main.Core.Events.User;
using Ncqrs.Domain;

namespace WB.Core.SharedKernels.DataCollection.Aggregates
{
    public class UserAR : AggregateRootMappedByConvention
    {
        private string email;

        private bool isUserLocked;

        private string passwordHash;

        private UserRoles[] roles;

        private UserLight supervisor;

        private string userName;

        public UserAR()
        {
        }

        public UserAR(
            Guid publicKey, 
            string userName, 
            string password, 
            string email, 
            UserRoles[] roles, 
            bool isLocked, 
            UserLight supervisor)
            : base(publicKey)
        {
            //// Check for uniqueness of person name and email!
            this.ApplyEvent(
                new NewUserCreated
                    {
                        Name = userName, 
                        Password = password, 
                        Email = email, 
                        IsLocked = isLocked, 
                        Roles = roles, 
                        Supervisor = supervisor, 
                        PublicKey = publicKey
                    });
        }

        public void ChangeUser(string email, bool isLocked, UserRoles[] roles, string passwordHash)
        {
            this.ApplyEvent(new UserChanged { Email = email, Roles = roles, PasswordHash = passwordHash});

            if (isLocked)
            {
                this.ApplyEvent(new UserLocked());
            }
            else
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

        protected void OnNewUserCreated(NewUserCreated e)
        {
            this.userName = e.Name;
            this.email = e.Email;
            this.passwordHash = e.Password;
            this.isUserLocked = e.IsLocked;
            this.roles = e.Roles;
            this.supervisor = e.Supervisor;
        }

        protected void OnUserLocked(UserLocked @event)
        {
            this.isUserLocked = true;
        }

        protected void OnUserUnlocked(UserUnlocked @event)
        {
            this.isUserLocked = false;
        }

        protected void OnUserChange(UserChanged e)
        {
            this.email = e.Email;
            this.roles = e.Roles;
            this.passwordHash = e.PasswordHash;
        }
    }
}