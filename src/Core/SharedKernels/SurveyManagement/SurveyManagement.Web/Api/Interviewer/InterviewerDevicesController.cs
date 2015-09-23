using System.Globalization;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.SurveyManagement.Web.Code;
using WB.Core.SharedKernels.SurveyManagement.Web.Models.User;
using WB.Core.SharedKernels.SurveyManagement.Web.Resources;
using WB.Core.SharedKernels.SurveyManagement.Web.Utils.Membership;
using WB.Core.Synchronization;
using WB.Core.Synchronization.SyncStorage;

namespace WB.Core.SharedKernels.SurveyManagement.Web.Api.Interviewer
{
    [ApiBasicAuth]
    [RoutePrefix("api/interviewer/v1/devices")]
    [ProtobufJsonSerializer]
    public class InterviewerDevicesController : ApiController
    {
        private readonly IGlobalInfoProvider globalInfoProvider;
        private readonly IUserWebViewFactory userInfoViewFactory;
        private readonly ISyncManager syncManager;
        private readonly ISyncLogger syncLogger;
        private readonly ISyncProtocolVersionProvider syncVersionProvider;

        public InterviewerDevicesController(
            IGlobalInfoProvider globalInfoProvider,
            IUserWebViewFactory userInfoViewFactory,
            ISyncManager syncManager,
            ISyncLogger syncLogger,
            ISyncProtocolVersionProvider syncVersionProvider)
        {
            this.globalInfoProvider = globalInfoProvider;
            this.userInfoViewFactory = userInfoViewFactory;
            this.syncManager = syncManager;
            this.syncLogger = syncLogger;
            this.syncVersionProvider = syncVersionProvider;
        }

        [HttpGet]
        [Route("current/{id}/{version}")]
        public bool IsDeviceLinkedToCurrentInterviewer(string id, int version)
        {
            this.ThrowIfInterviewerIncompatibleWithServer(version);

            var interviewerInfo = this.userInfoViewFactory.Load(new UserWebViewInputModel(this.globalInfoProvider.GetCurrentUser().Name, null));
            return interviewerInfo.DeviceId == id;
        }

        [HttpPost]
        [Route("link/{id}/{version:int}")]
        public HttpResponseMessage LinkCurrentInterviewerToDevice(string id, int version)
        {
            this.syncLogger.TraceHandshake(id.ToGuid(), this.globalInfoProvider.GetCurrentUser().Id, version.ToString(CultureInfo.InvariantCulture));

            var interviewerInfo = this.userInfoViewFactory.Load(new UserWebViewInputModel(this.globalInfoProvider.GetCurrentUser().Name, null));
            this.syncManager.LinkUserToDevice(interviewerId: this.globalInfoProvider.GetCurrentUser().Id,
                androidId: id,
                appVersion: version.ToString(CultureInfo.InvariantCulture), 
                oldDeviceId: interviewerInfo.DeviceId);

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