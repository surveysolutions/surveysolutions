using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RavenQuestionnaire.Core.Entities.SubEntities;
using RavenQuestionnaire.Core.Utility;

namespace RavenQuestionnaire.Core.Commands
{
    public class CreateNewUserCommand : ICommand
    {
        public string UserName { get; private set; }

        public string Password { get; private set; }

        public string Email { get; private set; }

        public bool IsLocked { get; private set; }

        public UserRoles Role { get; private set; }

        public string SupervisorId { get; private set; }

        public string LocationId { get; private set; }

        public CreateNewUserCommand(string userName, string email, string password, UserRoles role, bool isLocked, string supervisorId, string locationId)
        {
            this.UserName = userName;
            this.Email = email;
            this.Password = password;
            this.IsLocked = isLocked;
            this.Role = role;
            if (!string.IsNullOrEmpty(supervisorId))
            {
                this.SupervisorId = IdUtil.CreateUserId(supervisorId);
            }
            this.LocationId = IdUtil.CreateLocationId(locationId);
        }
    }
}
