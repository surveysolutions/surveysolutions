#nullable enable
using System;
using System.Collections.Generic;
using WB.Core.SharedKernels.DataCollection.WebApi;

namespace WB.UI.Headquarters.Code.UsersManagement
{
    public class UserManagementListItem
    {
        public Guid UserId { get; set; }
        public string? UserName { get; set; }
        public DateTime CreationDate { get; set; }
        public string? Email { get; set; }
        public List<WorkspaceApiView>? Workspaces { get; set; }
        public bool IsLocked { get; set; }
        public string? FullName { get; set; }
        public bool IsArchived { get; set; }
    }
}
