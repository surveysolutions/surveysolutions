using System.Globalization;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using WB.Core.BoundedContexts.Headquarters.Commands;
using WB.Core.BoundedContexts.Headquarters.Documents;
using WB.Core.BoundedContexts.Headquarters.OwinSecurity;
using WB.Core.BoundedContexts.Headquarters.Services;
using WB.Core.BoundedContexts.Headquarters.Views.SynchronizationLog;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernels.DataCollection;
using WB.UI.Headquarters.Code;

namespace WB.Core.SharedKernels.SurveyManagement.Web.Api.Interviewer
{
    public class DevicesControllerBase : ApiController
    {
        protected readonly IAuthorizedUser authorizedUser;
        private readonly ISyncProtocolVersionProvider syncVersionProvider;
        private readonly ICommandService commandService;
        private readonly IReadSideRepositoryReader<TabletDocument> devicesRepository;
        private readonly HqUserManager userManager;

        public DevicesControllerBase(
            IAuthorizedUser authorizedUser,
            ISyncProtocolVersionProvider syncVersionProvider,
            ICommandService commandService,
            IReadSideRepositoryReader<TabletDocument> devicesRepository,
            HqUserManager userManager)
        {
            this.authorizedUser = authorizedUser;
            this.syncVersionProvider = syncVersionProvider;
            this.commandService = commandService;
            this.devicesRepository = devicesRepository;
            this.userManager = userManager;
        }
        
        [WriteToSyncLog(SynchronizationLogType.CanSynchronize)]
        public virtual HttpResponseMessage CanSynchronize(string id, int version)
        {
            int supervisorRevisionNumber = this.syncVersionProvider.GetProtocolVersion();
            int supervisorShiftVersionNumber = this.syncVersionProvider.GetLastNonUpdatableVersion();

            if (version != supervisorRevisionNumber || version < supervisorShiftVersionNumber)
            {
                return this.Request.CreateResponse(HttpStatusCode.UpgradeRequired);
            }
            
            return this.authorizedUser.DeviceId != id
                ? this.Request.CreateResponse(HttpStatusCode.Forbidden)
                : this.Request.CreateResponse(HttpStatusCode.OK);
        }
        
        [WriteToSyncLog(SynchronizationLogType.LinkToDevice)]
        public virtual HttpResponseMessage LinkCurrentInterviewerToDevice(string id, int version)
        {
            var interviewerEngineVersion = version.ToString(CultureInfo.InvariantCulture);
            var deviceId = id.ToGuid();
            var device = this.devicesRepository.GetById(deviceId);
            if (device == null)
            {
                this.commandService.Execute(new RegisterTabletCommand(deviceId, this.authorizedUser.Id, interviewerEngineVersion, id));
            }

            this.userManager.LinkDeviceToCurrentInterviewer(id);

            return this.Request.CreateResponse(HttpStatusCode.OK);
        }
    }
}