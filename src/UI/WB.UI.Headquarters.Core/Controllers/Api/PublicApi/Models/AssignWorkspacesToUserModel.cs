using System;
using System.ComponentModel.DataAnnotations;

namespace WB.UI.Headquarters.Controllers.Api.PublicApi.Models
{
    public class AssignWorkspacesToUserModel
    {
        public AssignWorkspacesToUserModel()
        {
            Workspaces = Array.Empty<string>();
        }

        [Required]
        public Guid[] UserIds { get; set; }

        [Required]
        public string[] Workspaces { get; set; }

        public AssignWorkspacesMode Mode { get; set; } = AssignWorkspacesMode.Assign;
    }
}
