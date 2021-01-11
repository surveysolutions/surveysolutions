#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using WB.Core.BoundedContexts.Headquarters.Views.User;
using WB.Core.SharedKernels.DataCollection.WebApi;

namespace WB.UI.Headquarters.Code.UsersManagement
{
    public class UserManagementListItem
    {
        public UserManagementListItem(Guid userId, string userName, ICollection<HqRole> role)
        {
            UserId = userId;
            UserName = userName;
            Role = role.First().Name;
        }

        public Guid UserId { get; set; }
        public string UserName { get; set; }
        public DateTime CreationDate { get; set; }
        public string? Email { get; set; }
        public List<WorkspaceApiView>? Workspaces { get; set; }
        public bool IsLocked { get; set; }
        public string? FullName { get; set; }
        public string? Phone { get; set; }
        public string Role { get; set; }
        public bool IsArchived { get; set; }
    }
}
