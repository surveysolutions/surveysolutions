using WB.UI.Headquarters.Code;
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

namespace WB.UI.Headquarters.Controllers.Api.DataCollection.Interviewer.v2
{
    [Authorize(Roles ="Interviewer")]
    [Route("api/interviewer/v2/devices")]
    public class DevicesApiV2Controller : DevicesControllerBase
    {
        public DevicesApiV2Controller(
            IInterviewerSyncProtocolVersionProvider syncVersionProvider,
            IUserToDeviceService userToDeviceService,
            IAuthorizedUser authorizedUser,
            IDeviceSyncInfoRepository deviceSyncInfoRepository,
            IPlainStorageAccessor<SynchronizationLogItem> syncLogRepository,
            IUserRepository userRepository
            ) : base(
                authorizedUser: authorizedUser,
                syncVersionProvider: syncVersionProvider,
                userToDeviceService: userToDeviceService,
                userRepository: userRepository,
                deviceSyncInfoRepository:deviceSyncInfoRepository,
                syncLogRepository: syncLogRepository
            )
        {
        }

        [HttpGet]
        [Route("current/{id}/{version}")]
        [WriteToSyncLog(SynchronizationLogType.CanSynchronize)]
        public override IActionResult CanSynchronize(string id, int version) => base.CanSynchronize(id, version);

        [HttpPost]
        [Route("link/{id}/{version:int}")]
        [WriteToSyncLog(SynchronizationLogType.LinkToDevice)]
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
