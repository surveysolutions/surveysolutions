using System.Threading;
using System.Threading.Tasks;
using WB.Core.BoundedContexts.Interviewer.Services;
using WB.Core.Infrastructure.FileSystem;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure;
namespace WB.Core.BoundedContexts.Interviewer.Implementation.Services
{
    public class MapSynchronizer : IMapSynchronizer
    {
        private ISynchronizationService synchronizationService;
        private IFileSystemAccessor fileSystemAccessor;

        public MapSynchronizer(IFileSystemAccessor fileSystemAccessor, ISynchronizationService synchronizationService)
        {
            this.fileSystemAccessor = fileSystemAccessor;
            this.synchronizationService = synchronizationService;
        }

        public Task SyncMaps(string workingDirectory, CancellationToken cancellationToken)
        {
            if (!this.fileSystemAccessor.IsDirectoryExists(workingDirectory))
                this.fileSystemAccessor.CreateDirectory(workingDirectory);

            return synchronizationService.SyncMaps(workingDirectory, cancellationToken);
        }
        
    }
}
