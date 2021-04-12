using System;
using System.ComponentModel.DataAnnotations;

namespace WB.UI.Headquarters.Controllers.Api.PublicApi.Models
{
    public class AssignWorkspacesToUserModel
    {
        public AssignWorkspacesToUserModel()
        {
            Workspaces = Array.Empty<AssignWorkspaceInfo>();
        }

        [Required]
        public Guid[] UserIds { get; set; }

        [Required]
        public AssignWorkspaceInfo[] Workspaces { get; set; }

        public AssignWorkspacesMode Mode { get; set; } = AssignWorkspacesMode.Assign;
    }
}
