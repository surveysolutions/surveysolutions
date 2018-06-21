using System;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using WB.Core.BoundedContexts.Headquarters.OwinSecurity;
using WB.Core.BoundedContexts.Headquarters.Services;
using WB.Core.SharedKernels.DataCollection;

namespace WB.UI.Headquarters.API.DataCollection
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
        
        public virtual HttpResponseMessage LinkCurrentResponsibleToDevice(string id, int version)
        {
            this.userManager.LinkDeviceToInterviewerOrSupervisor(this.authorizedUser.Id, id, DateTime.UtcNow);

            return this.Request.CreateResponse(HttpStatusCode.OK);
        }
    }
}
