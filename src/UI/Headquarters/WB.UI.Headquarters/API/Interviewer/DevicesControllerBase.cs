using System;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using WB.Core.BoundedContexts.Headquarters.OwinSecurity;
using WB.Core.BoundedContexts.Headquarters.Services;
using WB.Core.BoundedContexts.Headquarters.Views.SynchronizationLog;
using WB.Core.SharedKernels.DataCollection;
using WB.UI.Headquarters.Code;

namespace WB.Core.SharedKernels.SurveyManagement.Web.Api.Interviewer
{
    public class DevicesControllerBase : ApiController
    {
        protected readonly IAuthorizedUser authorizedUser;
        private readonly ISyncProtocolVersionProvider syncVersionProvider;
        private readonly HqUserManager userManager;

        public DevicesControllerBase(
            IAuthorizedUser authorizedUser,
            ISyncProtocolVersionProvider syncVersionProvider,
            HqUserManager userManager)
        {
            this.authorizedUser = authorizedUser;
            this.syncVersionProvider = syncVersionProvider;
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
            this.userManager.LinkDeviceToInterviewer(this.authorizedUser.Id, id, DateTime.UtcNow);

            return this.Request.CreateResponse(HttpStatusCode.OK);
        }
    }
}