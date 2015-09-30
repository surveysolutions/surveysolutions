using System.Globalization;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Commands.User;
using WB.Core.SharedKernels.SurveyManagement.Views.SynchronizationLog;
using WB.Core.SharedKernels.SurveyManagement.Web.Code;
using WB.Core.SharedKernels.SurveyManagement.Web.Models.User;
using WB.Core.SharedKernels.SurveyManagement.Web.Resources;
using WB.Core.SharedKernels.SurveyManagement.Web.Utils.Membership;
using WB.Core.Synchronization.Commands;
using WB.Core.Synchronization.Documents;

namespace WB.Core.SharedKernels.SurveyManagement.Web.Api.Interviewer
{
    [ApiBasicAuth]
    [RoutePrefix("api/interviewer/v1/devices")]
    [ProtobufJsonSerializer]
    public class InterviewerDevicesController : ApiController
    {
        private readonly IGlobalInfoProvider globalInfoProvider;
        private readonly IUserWebViewFactory userInfoViewFactory;
        private readonly ISyncProtocolVersionProvider syncVersionProvider;
        private readonly ICommandService commandService;
        private readonly IReadSideRepositoryReader<TabletDocument> devicesRepository;

        public InterviewerDevicesController(
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

        [HttpGet]
        [Route("current/{id}/{version}")]
        [WriteToSyncLog(SynchronizationLogType.CanSynchronize)]
        public bool IsDeviceLinkedToCurrentInterviewer(string id, int version)
        {
            this.ThrowIfInterviewerIncompatibleWithServer(version);

            var interviewerInfo = this.userInfoViewFactory.Load(new UserWebViewInputModel(this.globalInfoProvider.GetCurrentUser().Name, null));
            return interviewerInfo.DeviceId == id;
        }

        [HttpPost]
        [Route("link/{id}/{version:int}")]
        [WriteToSyncLog(SynchronizationLogType.LinkToDevice)]
        public HttpResponseMessage LinkCurrentInterviewerToDevice(string id, int version)
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

            return Request.CreateResponse(HttpStatusCode.OK);
        }

        public void ThrowIfInterviewerIncompatibleWithServer(int version)
        {
            int supervisorRevisionNumber = this.syncVersionProvider.GetProtocolVersion();
            int supervisorShiftVersionNumber = this.syncVersionProvider.GetLastNonUpdatableVersion();

            if (version != supervisorRevisionNumber || version < supervisorShiftVersionNumber)
            {
                throw new HttpResponseException(new HttpResponseMessage(HttpStatusCode.NotAcceptable)
                {
                    ReasonPhrase = InterviewerSyncStrings.InterviewerApplicationShouldBeUpdated
                });
            }
        }
    }
}