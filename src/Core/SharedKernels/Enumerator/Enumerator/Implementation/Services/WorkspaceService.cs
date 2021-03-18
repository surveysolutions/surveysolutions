using System.Linq;
using WB.Core.Infrastructure.FileSystem;
using WB.Core.SharedKernels.Enumerator.Services;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure.Storage;
using WB.Core.SharedKernels.Enumerator.Services.Workspace;
using WB.Core.SharedKernels.Enumerator.Views;

namespace WB.Core.SharedKernels.Enumerator.Implementation.Services
{
    public class WorkspaceService : IWorkspaceService
    {
        private readonly IPlainStorage<WorkspaceView> workspaceRepository;
        private readonly SqliteSettings settings;
        private readonly IFileSystemAccessor fileSystemAccessor;

        public WorkspaceService(IPlainStorage<WorkspaceView> workspaceRepository,
            SqliteSettings settings,
            IFileSystemAccessor fileSystemAccessor)
        {
            this.workspaceRepository = workspaceRepository;
            this.settings = settings;
            this.fileSystemAccessor = fileSystemAccessor;
        }

        public void Save(WorkspaceView[] workspaces)
        {
            var currentWorkspaces = workspaceRepository.LoadAll();
            var removedWorkspaces = currentWorkspaces.Where(w => workspaces.All(nw => nw.Name != w.Name)).ToList();

            if (removedWorkspaces.Count > 0)
            {
                foreach (var removedWorkspace in removedWorkspaces)
                {
                    var workspaceDirectory = fileSystemAccessor.CombinePath(settings.PathToDatabaseDirectory, removedWorkspace.Name);
                    fileSystemAccessor.DeleteDirectory(workspaceDirectory);
                }
                workspaceRepository.Remove(removedWorkspaces);
            }

            workspaceRepository.Store(workspaces);
        }

        public WorkspaceView[] GetAll()
        {
            return workspaceRepository.LoadAll()
                .Where(s => s.Disabled == false)
                .ToArray();
        }

        public WorkspaceView GetByName(string workspace)
        {
            return workspaceRepository.GetById(workspace);
        }
    }
}