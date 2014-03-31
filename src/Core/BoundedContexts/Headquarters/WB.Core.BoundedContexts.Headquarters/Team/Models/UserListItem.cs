using System;
using System.Collections.Generic;
using Main.Core.Entities.SubEntities;

namespace WB.Core.BoundedContexts.Headquarters.Team.Models
{
    public class UserListItem
    {
        public bool IsLocked { get; set; }
        
        public string CreationDate { get; set; }

        public string Email { get; set; }

        public Guid UserId { get; set; }

        public string UserName { get; set; }
        
        public List<UserRoles> Roles { get; set; }
    }
}