using System;
using System.Threading.Tasks;
using WB.Core.SharedKernels.Enumerator.OfflineSync.Messages;
using WB.Core.SharedKernels.Enumerator.OfflineSync.Services;
using WB.Core.SharedKernels.Enumerator.Services;
using WB.Core.SharedKernels.Enumerator.Views;

namespace WB.Core.BoundedContexts.Supervisor.Services.Implementation.OfflineSyncHandlers
{
    public class SupervisorTabletInfoHandler : IHandleCommunicationMessage
    {
        private readonly ITabletInfoService tabletInfoStorage;

        public SupervisorTabletInfoHandler(ITabletInfoService tabletInfoStorage)
        {
            this.tabletInfoStorage = tabletInfoStorage;
        }

        public void Register(IRequestHandler requestHandler)
        {
            requestHandler.RegisterHandler<UploadDeviceInfoRequest, OkResponse>(UploadTableInfo);
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
