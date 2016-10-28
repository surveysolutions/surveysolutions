using System.Globalization;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using WB.Core.BoundedContexts.Headquarters.Commands;
using WB.Core.BoundedContexts.Headquarters.Documents;
using WB.Core.BoundedContexts.Headquarters.Services;
using WB.Core.BoundedContexts.Headquarters.Views.SynchronizationLog;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Commands.User;
using WB.Core.SharedKernels.SurveyManagement.Web.Code;
using WB.Core.SharedKernels.SurveyManagement.Web.Models.User;
using WB.UI.Headquarters.Code;

namespace WB.Core.SharedKernels.SurveyManagement.Web.Api.Interviewer
{
    public class DevicesControllerBase : ApiController
    {
        private readonly IIdentityManager identityManager;
        private readonly ISyncProtocolVersionProvider syncVersionProvider;
        private readonly ICommandService commandService;
        private readonly IReadSideRepositoryReader<TabletDocument> devicesRepository;

        public DevicesControllerBase(
            IIdentityManager identityManager,
            ISyncProtocolVersionProvider syncVersionProvider,
            ICommandService commandService,
            IReadSideRepositoryReader<TabletDocument> devicesRepository)
        {
            this.identityManager = identityManager;
            this.syncVersionProvider = syncVersionProvider;
            this.commandService = commandService;
            this.devicesRepository = devicesRepository;
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
            
            return this.identityManager.CurrentUserDeviceId != id
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
                this.commandService.Execute(new RegisterTabletCommand(deviceId, this.identityManager.CurrentUserId, interviewerEngineVersion, id));
            }

            this.identityManager.LinkDeviceToCurrentInterviewer(deviceId);

            return this.Request.CreateResponse(HttpStatusCode.OK);
        }
    }
}