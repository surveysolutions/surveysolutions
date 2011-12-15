using RavenQuestionnaire.Core.Entities.SubEntities;
using RavenQuestionnaire.Core.Utility;

namespace RavenQuestionnaire.Core.Commands
{
    public class UpdateUserCommand : ICommand
    {
        public string UserId { get; private set; }

    //    public string Password { get; private set; }

        public string Email { get; private set; }

        public bool IsLocked { get; private set; }

        public UserRoles[] Roles { get; private set; }

        public string SupervisorId { get; private set; }

        public string LocationId { get; private set; }

        public UpdateUserCommand(string userId, string email,/* string password,*/ bool isLocked, UserRoles[] rolesList, string supervisorId, string locationId)
        {
            this.UserId = IdUtil.CreateUserId(userId);
         //   this.Password = password;
            this.Email = email;
            this.IsLocked = isLocked;
            this.Roles = rolesList;
            if (!string.IsNullOrEmpty(supervisorId))
            {
                this.SupervisorId = IdUtil.CreateUserId(supervisorId);
            }
            this.LocationId = IdUtil.CreateLocationId(locationId);
        }
    }
}
