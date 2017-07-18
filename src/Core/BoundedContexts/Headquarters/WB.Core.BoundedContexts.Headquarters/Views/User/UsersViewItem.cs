using System;
using Main.Core.Entities.SubEntities;

namespace WB.Core.BoundedContexts.Headquarters.Views.User
{
    public class UsersViewItem
    {
        public Guid UserId { get; set; }

        public string UserName { get; set; }

        public UserRoles Role { get; set; }
    }
}