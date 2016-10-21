using System.Globalization;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using WB.Core.BoundedContexts.Headquarters.Commands;
using WB.Core.BoundedContexts.Headquarters.Documents;
using WB.Core.BoundedContexts.Headquarters.Views.SynchronizationLog;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Commands.User;
using WB.Core.SharedKernels.SurveyManagement.Web.Code;
using WB.Core.SharedKernels.SurveyManagement.Web.Models.User;
using WB.Core.SharedKernels.SurveyManagement.Web.Utils.Membership;
using WB.UI.Headquarters.Code;

namespace WB.Core.SharedKernels.SurveyManagement.Web.Api.Interviewer
{
    public class DevicesControllerBase : ApiController
    {
        private readonly IGlobalInfoProvider globalInfoProvider;
        private readonly IUserWebViewFactory userInfoViewFactory;
        private readonly ISyncProtocolVersionProvider syncVersionProvider;
        private readonly ICommandService commandService;
        private readonly IReadSideRepositoryReader<TabletDocument> devicesRepository;

        public DevicesControllerBase(
            IGlobalInfoProvider globalInfoProvider,
            IUserWebViewFactory userInfoViewFactory,
            ISyncProtocolVersionProvider syncVersionProvider,
            ICommandService commandService,
            IReadSideRepositoryReader<TabletDocument> devicesRepository)
        {
            this.globalInfoProvider = globalInfoProvider;
            this.userInfoViewFactory = userInfoViewFactory;
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

            var interviewerInfo = this.userInfoViewFactory.Load(new UserWebViewInputModel(this.globalInfoProvider.GetCurrentUser().Name, null));
            return interviewerInfo.DeviceId != id
                ? this.Request.CreateResponse(HttpStatusCode.Forbidden)
                : this.Request.CreateResponse(HttpStatusCode.OK);
        }
        
        [WriteToSyncLog(SynchronizationLogType.LinkToDevice)]
        public virtual HttpResponseMessage LinkCurrentInterviewerToDevice(string id, int version)
        {
            var interviewerEngineVersion = version.ToString(CultureInfo.InvariantCulture);
            var interviewerId = this.globalInfoProvider.GetCurrentUser().Id;
            var deviceId = id.ToGuid();
            var device = this.devicesRepository.GetById(deviceId);
            if (device == null)
            {
                this.commandService.Execute(new RegisterTabletCommand(deviceId, interviewerId, interviewerEngineVersion, id));
            }
            this.commandService.Execute(new LinkUserToDevice(interviewerId, id));

            return this.Request.CreateResponse(HttpStatusCode.OK);
        }
    }
}