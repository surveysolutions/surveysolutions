using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WB.Core.BoundedContexts.Headquarters.Repositories;
using WB.Core.BoundedContexts.Headquarters.Services;
using WB.Core.BoundedContexts.Headquarters.Users;
using WB.Core.BoundedContexts.Headquarters.Views.SynchronizationLog;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.WebApi;
using WB.UI.Headquarters.Code;

namespace WB.UI.Headquarters.Controllers.Api.DataCollection.Supervisor.v1
{
    [Authorize(Roles = "Supervisor")]
    [Route("api/supervisor/v1/devices")]
    public class DevicesApiV1Controller : DevicesControllerBase
    {
        public DevicesApiV1Controller(
            IInterviewerSyncProtocolVersionProvider syncVersionProvider,
            IUserToDeviceService userToDeviceService,
            IAuthorizedUser authorizedUser,
            IDeviceSyncInfoRepository deviceSyncInfoRepository,
            IPlainStorageAccessor<SynchronizationLogItem> syncLogRepository,
            IUserRepository userRepository) : base(
            authorizedUser: authorizedUser,
            syncVersionProvider: syncVersionProvider,
            userToDeviceService: userToDeviceService,
            userRepository: userRepository,
            deviceSyncInfoRepository: deviceSyncInfoRepository,
            syncLogRepository: syncLogRepository)
        {
        }

        [HttpGet]
        [WriteToSyncLog(SynchronizationLogType.CanSynchronize)]
        [Route("current/{id}/{version}")]
        public override IActionResult CanSynchronize(string id, int version) => base.CanSynchronize(id, version);

        [HttpPost]
        [WriteToSyncLog(SynchronizationLogType.LinkToDevice)]
        [Route("link/{id}/{version:int}")]
        public override Task<IActionResult> LinkCurrentResponsibleToDevice(string id, int version) => base.LinkCurrentResponsibleToDevice(id, version);

        [HttpPost]
        [Route("info")]
        public override Task<IActionResult> Info([FromBody]DeviceInfoApiView info) => base.Info(info);

        [HttpPost]
        [Route("statistics")]
        public override IActionResult Statistics([FromBody]SyncStatisticsApiView statistics) => base.Statistics(statistics);

        [HttpPost]
        [Route("exception")]
        public override IActionResult UnexpectedException([FromBody]UnexpectedExceptionApiView exception) => base.UnexpectedException(exception);
    }
}
