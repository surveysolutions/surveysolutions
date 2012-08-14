using System;
using System.Collections.Generic;
using System.Linq;
using RavenQuestionnaire.Core.Entities.SubEntities;
using RavenQuestionnaire.Core.Utility;

namespace RavenQuestionnaire.Core.Views.User
{
    public class UserView
    {
        public UserView()
        {
        }

        public UserView(
            Guid publicKey,
            string id,
            string username,
            string password,
            string email,
            DateTime creationDate,
            IEnumerable<UserRoles> roles,
            bool isLocked,
            UserLight supervisor,
            string locationId
            )
        {
            this.PublicKey = publicKey;
            this.UserId = IdUtil.ParseId(id);
            this.UserName = username;
            this.Password = password;
            this.Email = email;
            this.CreationDate = creationDate;
            this.Roles = roles;
            this.IsLocked = isLocked;
            this.Supervisor = supervisor;
            this.LocationId = IdUtil.ParseId(locationId);
        }
        public Guid PublicKey { get; set; }
        public string UserId { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }
        public string Email { get; set; }
        public bool IsLocked { get; set; }
        public UserLight Supervisor { get; set; }

        public UserRoles PrimaryRole
        {
            get
            {
                if (_primaryRole.HasValue)
                    return _primaryRole.Value;
                if (Roles != null)
                    return Roles.FirstOrDefault();
                return UserRoles.User;
            }
            set { _primaryRole = value; }
        }

        private UserRoles? _primaryRole;

        public DateTime CreationDate { get; set; }

        public IEnumerable<UserRoles> Roles { get; set; }

        public string LocationId { get; set; }

        public static UserView New()
        {
            return new UserView(Guid.Empty, null, null, null, null, DateTime.UtcNow, new UserRoles[] {UserRoles.User}, false, null,
                                null);
        }
    }
}
