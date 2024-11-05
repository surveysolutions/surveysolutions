using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using WB.Core.BoundedContexts.Supervisor.Views;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.SharedKernels.DataCollection.Views.InterviewerAuditLog;
using WB.Core.SharedKernels.Enumerator.OfflineSync.Messages;
using WB.Core.SharedKernels.Enumerator.OfflineSync.Services;
using WB.Core.SharedKernels.Enumerator.Services;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure.Storage;
using WB.Core.SharedKernels.Enumerator.Views;

namespace WB.Core.BoundedContexts.Supervisor.Services.Implementation.OfflineSyncHandlers
{
    public class SupervisorGeoTrackingHandler : IHandleCommunicationMessage
    {
        public SupervisorGeoTrackingHandler(
            )
        {
        }

        public void Register(IRequestHandler requestHandler)
        {
            requestHandler.RegisterHandler<UploadGeoTrackingPackageRequest, OkResponse>(UploadGeoTracking);
        }

        public Task<OkResponse> UploadGeoTracking(UploadGeoTrackingPackageRequest request)
        {
            

            return OkResponse.Task;
        }
    }
}
