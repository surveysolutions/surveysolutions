#nullable enable
using WB.Core.BoundedContexts.Headquarters.Workspaces;

namespace WB.UI.Headquarters.Controllers.Api.PublicApi.Models
{
    public static class WorkspaceMapper
    {
        public static WorkspaceApiView ToApiView(this Workspace workspace) => new WorkspaceApiView
        {
            Name = workspace.Name,
            DisplayName = workspace.DisplayName,
            DisabledAtUtc = workspace.DisabledAtUtc,
            CreatedAtUtc = workspace.CreatedAtUtc
        };
    }
}
