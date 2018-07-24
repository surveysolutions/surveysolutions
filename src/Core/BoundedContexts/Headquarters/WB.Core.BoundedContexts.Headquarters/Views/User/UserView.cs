using System;
using System.Collections.Generic;
using System.Linq;
using Main.Core.Entities.SubEntities;

namespace WB.Core.BoundedContexts.Headquarters.Views.User
{
    public class UserView
    {
        public DateTime CreationDate { get; set; }
        public string Email { get; set; }
        public bool IsLockedByHQ { get; set; }
        public bool IsArchived { get; set; }
        public bool IsLockedBySupervisor { get; set; }
        public Guid PublicKey { get; set; }
        public UserLight Supervisor { get; set; }
        public string UserName { get; set; }
        public ISet<UserRoles> Roles { get; set; } = new HashSet<UserRoles>();
        public string PersonName { get; set; }
        public string PhoneNumber { get; set; }

        public string SecurityStamp { get; set; }

        public bool IsSupervisor()
        {
            return Roles.Any(role => role == UserRoles.Supervisor);
        }

        public UserLight GetUseLight()
        {
            return new UserLight(this.PublicKey, this.UserName);
        }

        public bool IsInterviewer()
        {
            return Roles.Any(x => x == UserRoles.Interviewer);
        }
    }
}
