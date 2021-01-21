using MediatR;

namespace WB.UI.Headquarters.Code.Workspaces
{
    public class GetWorkspaceStatusInformation : IRequest<WorkspaceStatusInformation>
    {
        public GetWorkspaceStatusInformation(string workspaceName)
        {
            WorkspaceName = workspaceName;
        }

        public string WorkspaceName { get; set; }
    }
}