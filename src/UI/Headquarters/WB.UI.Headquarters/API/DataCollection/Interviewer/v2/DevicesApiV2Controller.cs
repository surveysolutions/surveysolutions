using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using Main.Core.Entities.SubEntities;
using WB.Core.BoundedContexts.Headquarters.OwinSecurity;
using WB.Core.BoundedContexts.Headquarters.Repositories;
using WB.Core.BoundedContexts.Headquarters.Services;
using WB.Core.BoundedContexts.Headquarters.Views.SynchronizationLog;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.WebApi;
using WB.UI.Headquarters.Code;

namespace WB.UI.Headquarters.API.DataCollection.Interviewer.v2
{
    [ApiBasicAuth(UserRoles.Interviewer)]
    public class DevicesApiV2Controller : DevicesControllerBase
    {
        public DevicesApiV2Controller(
            IInterviewerSyncProtocolVersionProvider syncVersionProvider,
            IAuthorizedUser authorizedUser,
            IDeviceSyncInfoRepository deviceSyncInfoRepository,
            IPlainStorageAccessor<SynchronizationLogItem> syncLogRepository,
            HqUserManager userManager) : base(
                authorizedUser: authorizedUser,
                syncVersionProvider: syncVersionProvider,
                userManager: userManager,
                deviceSyncInfoRepository:deviceSyncInfoRepository,
                syncLogRepository: syncLogRepository
            )
        {
        }

        [HttpGet]
        [WriteToSyncLog(SynchronizationLogType.CanSynchronize)]
        public override HttpResponseMessage CanSynchronize(string id, int version) => base.CanSynchronize(id, version);

        [HttpPost]
        [WriteToSyncLog(SynchronizationLogType.LinkToDevice)]
        public override HttpResponseMessage LinkCurrentResponsibleToDevice(string id, int version) => base.LinkCurrentResponsibleToDevice(id, version);

        [HttpPost]
        public override Task<IHttpActionResult> Info(DeviceInfoApiView info) => base.Info(info);

        [HttpPost]
        public override long Statistics(SyncStatisticsApiView statistics) => base.Statistics(statistics);

        [HttpPost]
        public override IHttpActionResult UnexpectedException(UnexpectedExceptionApiView exception) => base.UnexpectedException(exception);
    }
}
