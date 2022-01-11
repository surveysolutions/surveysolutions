using System;
using System.ComponentModel.DataAnnotations;

namespace WB.UI.Headquarters.Controllers.Api.PublicApi.Models
{
    public class AssignWorkspaceInfo
    {
        public AssignWorkspaceInfo()
        {
        }

        public AssignWorkspaceInfo(string workspace, Guid? supervisorId = null)
        {
            Workspace = workspace;
            SupervisorId = supervisorId;
        }

        [Required]
        public string Workspace { get; set; }

        public Guid? SupervisorId { get; set; }
    }
}