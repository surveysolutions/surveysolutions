using System.Linq;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure.Storage;
using WB.Core.SharedKernels.Enumerator.Services.Workspace;
using WB.Core.SharedKernels.Enumerator.Views;

namespace WB.Core.SharedKernels.Enumerator.Implementation.Services
{
    public class WorkspaceService : IWorkspaceService
    {
        private readonly IPlainStorage<WorkspaceView> workspaceRepository;

        public WorkspaceService(IPlainStorage<WorkspaceView> workspaceRepository)
        {
            this.workspaceRepository = workspaceRepository;
        }

        public void Save(WorkspaceView[] workspaces)
        {
            workspaceRepository.Store(workspaces);
        }

        public WorkspaceView[] GetAll()
        {
            return workspaceRepository.LoadAll().ToArray();
        }

        public WorkspaceView GetByName(string workspace)
        {
            return workspaceRepository.FirstOrDefault(w => w.Name == workspace);
        }
    }
}