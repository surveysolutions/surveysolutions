using System;
using System.Collections.Generic;
using System.Linq;
using Main.Core.Entities.SubEntities;

namespace Main.Core.View.User
{
    public class UserView
    {
        private UserRoles? primaryRole;

        public UserView()
        {
        }

        public UserView(Guid publicKey, string userName, string password, string email, DateTime creationDate, 
            IEnumerable<UserRoles> roles, bool isLocked, UserLight supervisor)
        {
            this.PublicKey = publicKey;
            this.UserName = userName;
            this.Password = password;
            this.Email = email;
            this.CreationDate = creationDate;
            this.Roles = roles;
            this.IsLocked = isLocked;
            this.Supervisor = supervisor;
        }

        public DateTime CreationDate { get; set; }

        public string Email { get; set; }

        public bool IsLocked { get; set; }

        public string Password { get; set; }

        public UserRoles PrimaryRole
        {
            get
            {
                if (this.primaryRole.HasValue) 
                    return this.primaryRole.Value;
                if (this.Roles != null) 
                    return this.Roles.FirstOrDefault();
                return UserRoles.User;
            }

            set
            {
                this.primaryRole = value;
            }
        }

        public Guid PublicKey { get; set; }

        public IEnumerable<UserRoles> Roles { get; set; }

        public UserLight Supervisor { get; set; }

        public string UserName { get; set; }

        public static UserView New()
        {
            return new UserView(
                Guid.Empty, null, null, null, DateTime.UtcNow, new[] { UserRoles.User }, false, null);
        }
    }
}