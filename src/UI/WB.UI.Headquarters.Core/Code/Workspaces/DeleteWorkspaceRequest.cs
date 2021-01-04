using MediatR;

namespace WB.UI.Headquarters.Code.Workspaces
{
    public class DeleteWorkspaceRequest : IRequest<DeleteWorkspaceResponse>
    {
        public DeleteWorkspaceRequest(string workspaceName)
        {
            WorkspaceName = workspaceName;
        }

        public string WorkspaceName { get; }
    }
}