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
        private readonly IExecuteInWorkspaceService executeInWorkspaceService;

        public WorkspaceService(IPlainStorage<WorkspaceView> workspaceRepository,
            SqliteSettings settings,
            IFileSystemAccessor fileSystemAccessor,
            IExecuteInWorkspaceService executeInWorkspaceService
            )
        {
            this.workspaceRepository = workspaceRepository;
            this.settings = settings;
            this.fileSystemAccessor = fileSystemAccessor;
            this.executeInWorkspaceService = executeInWorkspaceService;
        }

        public void Save(WorkspaceView[] workspaces)
        {
            var currentWorkspaces = workspaceRepository.LoadAll();
            var removedWorkspaces = currentWorkspaces.Where(w => workspaces.All(nw => nw.Name != w.Name)).ToList();

            if (removedWorkspaces.Count > 0)
            {
                foreach (var removedWorkspace in removedWorkspaces)
                {
                    var workspaceDirectory = fileSystemAccessor.CombinePath(settings.PathToRootDirectory, removedWorkspace.Name);
                    if (fileSystemAccessor.IsDirectoryExists(workspaceDirectory))
                    {
                        executeInWorkspaceService.Execute(removedWorkspace, serviceProvider =>
                        {
                            var interviewViewRepository = (IPlainStorage<InterviewView>)serviceProvider.GetService(typeof(IPlainStorage<InterviewView>));
                            var interviewsCount = interviewViewRepository.Count();
                            if (interviewsCount > 0)
                            {
                                var assignmentsStorage = (IAssignmentDocumentsStorage)serviceProvider.GetService(typeof(IAssignmentDocumentsStorage));
                                assignmentsStorage.RemoveAll();                            
                            }
                            else
                            {
                                fileSystemAccessor.DeleteDirectory(workspaceDirectory);
                                workspaceRepository.Remove(removedWorkspaces);
                            }
                        });
                    }
                }
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
