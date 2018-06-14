using System.Threading;
using System.Threading.Tasks;
using WB.Core.GenericSubdomains.Portable.Implementation;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure.FileSystem;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.WebApi;
using WB.Core.SharedKernels.Enumerator.Implementation.Services;
using WB.Core.SharedKernels.Enumerator.Services;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure;

namespace WB.UI.Supervisor.Services.Implementation
{
    public class SynchronizationService : EnumeratorSynchronizationService, ISupervisorSynchronizationService
    {
        protected override string ApiVersion => "v1";
        protected override string ApiUrl => "api/supervisor/";

        public SynchronizationService(IPrincipal principal, IRestService restService,
            ISupervisorSettings settings, ISyncProtocolVersionProvider syncProtocolVersionProvider,
            IFileSystemAccessor fileSystemAccessor, ICheckVersionUriProvider checkVersionUriProvider, ILogger logger) :
            base(principal, restService, settings, syncProtocolVersionProvider, fileSystemAccessor,
                checkVersionUriProvider, logger, settings)
        {
        }

        public Task<SupervisorApiView> GetSupervisorAsync(RestCredentials credentials = null, CancellationToken? token = null)
        {
            throw new System.NotImplementedException();
        }
    }
}
