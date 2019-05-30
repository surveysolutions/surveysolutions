using System;
using System.Threading.Tasks;
using WB.Core.BoundedContexts.Supervisor.Views;
using WB.Core.SharedKernels.Enumerator.OfflineSync.Messages;
using WB.Core.SharedKernels.Enumerator.OfflineSync.Services;
using WB.Core.SharedKernels.Enumerator.Services;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure.Storage;
using WB.Core.SharedKernels.Enumerator.Views;

namespace WB.Core.BoundedContexts.Supervisor.Services.Implementation.OfflineSyncHandlers
{
    public class SupervisorTabletInfoHandler : IHandleCommunicationMessage
    {
        private readonly ITabletInfoService tabletInfoStorage;
        private readonly IPlainStorage<SupervisorIdentity> supervisorsPlainStorage;

        public SupervisorTabletInfoHandler(ITabletInfoService tabletInfoStorage,
            IPlainStorage<SupervisorIdentity> supervisorsPlainStorage)
        {
            this.tabletInfoStorage = tabletInfoStorage;
            this.supervisorsPlainStorage = supervisorsPlainStorage;
        }

        public void Register(IRequestHandler requestHandler)
        {
            requestHandler.RegisterHandler<UploadDeviceInfoRequest, OkResponse>(UploadTableInfo);
            requestHandler.RegisterHandler<GetTenantIdRequest, GetTenantIdResponse>(GetTenantId);
        }

        private Task<GetTenantIdResponse> GetTenantId(GetTenantIdRequest arg)
        {
            var supervisor = supervisorsPlainStorage.FirstOrDefault();
            return Task.FromResult(new GetTenantIdResponse
            {
                TenantId = supervisor.TenantId
            });
        }

        private Task<OkResponse> UploadTableInfo(UploadDeviceInfoRequest request)
        {
            var info = request.DeviceInfo;

            info.UserId = request.UserId;
            info.ReceivedDate = DateTimeOffset.Now;

            this.tabletInfoStorage.Store(request.DeviceInfo);

            return Task.FromResult(new OkResponse());
        }
    }
}
