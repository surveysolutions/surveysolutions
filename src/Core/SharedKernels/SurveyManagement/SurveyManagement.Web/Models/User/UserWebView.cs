using System;
using System.Collections.Generic;
using System.Linq;
using Main.Core.Entities.SubEntities;

namespace WB.Core.SharedKernels.SurveyManagement.Web.Models.User
{
    public class UserWebView
    {
        private UserRoles? primaryRole;

        public UserWebView()
        {
        }

        public UserWebView(Guid publicKey, string userName, string password, string email, DateTime creationDate, 
            IEnumerable<UserRoles> roles, bool isLockedBySupervisor, bool isLockedByHQ, UserLight supervisor)
        {
            this.PublicKey = publicKey;
            this.UserName = userName;
            this.Password = password;
            this.Email = email;
            this.CreationDate = creationDate;
            this.Roles = roles;
            this.isLockedBySupervisor = isLockedBySupervisor;
            this.Supervisor = supervisor;
            this.IsLockedByHQ = isLockedByHQ;
        }

        public DateTime CreationDate { get; set; }

        public string Email { get; set; }

        public bool isLockedBySupervisor { get; set; }

        public bool IsLockedByHQ { get; set; }

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

        public static UserWebView New()
        {
            return new UserWebView(
                Guid.Empty, null, null, null, DateTime.UtcNow, new[] { UserRoles.User }, false, false, null);
        }
    }
}