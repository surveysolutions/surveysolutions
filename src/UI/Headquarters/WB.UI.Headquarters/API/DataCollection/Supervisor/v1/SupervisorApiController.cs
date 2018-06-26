using System;
using System.Net;
using System.Net.Http;
using System.Web.Hosting;
using System.Web.Http;
using Main.Core.Entities.SubEntities;
using WB.Core.BoundedContexts.Headquarters.Services;
using WB.Core.Infrastructure.FileSystem;
using WB.UI.Headquarters.Code;
using WB.UI.Shared.Web.Filters;

namespace WB.UI.Headquarters.API.DataCollection.Supervisor.v1
{
    public class SupervisorAppApiController : ApiController
    {
        private const string RESPONSEAPPLICATIONFILENAME = "supervisor.apk";
        private const string PHYSICALAPPLICATIONFILENAME = "supervisor.apk";
        private const string PHYSICALPATHTOAPPLICATION = "~/Client/";
        private readonly IFileSystemAccessor fileSystemAccessor;
        private readonly IAndroidPackageReader androidPackageReader;

        private readonly IAuthorizedUser authorizedUser;

        public SupervisorAppApiController(
            IAuthorizedUser authorizedUser, 
            IFileSystemAccessor fileSystemAccessor, 
            IAndroidPackageReader androidPackageReader)
        {
            this.authorizedUser = authorizedUser;
            this.fileSystemAccessor = fileSystemAccessor;
            this.androidPackageReader = androidPackageReader;
        }

        [HttpGet]
        public virtual int? GetLatestVersion()
        {
            string pathToInterviewerApp =
                this.fileSystemAccessor.CombinePath(HostingEnvironment.MapPath(PHYSICALPATHTOAPPLICATION), PHYSICALAPPLICATIONFILENAME);

            return !this.fileSystemAccessor.IsFileExists(pathToInterviewerApp)
                ? null
                : this.androidPackageReader.Read(pathToInterviewerApp).Version;
        }

        [ApiBasicAuth(UserRoles.Supervisor)]
        //[WriteToSyncLog(SynchronizationLogType.CanSynchronize)]
        [System.Web.Http.HttpGet]
        [ApiNoCache]
        public virtual HttpResponseMessage CheckCompatibility(string deviceId, int deviceSyncProtocolVersion)
        {
//            int serverSyncProtocolVersion = this.syncVersionProvider.GetProtocolVersion();
//            int lastNonUpdatableSyncProtocolVersion = this.syncVersionProvider.GetLastNonUpdatableVersion();
//
//            if (deviceSyncProtocolVersion < lastNonUpdatableSyncProtocolVersion)
//                return this.Request.CreateResponse(HttpStatusCode.UpgradeRequired);
//
//            var currentVersion = new Version(this.productVersion.ToString().Split(' ')[0]);
//            var supervisorVersion = GetSupervisorVersionFromUserAgent(this.Request);
//
//            if (supervisorVersion != null && supervisorVersion > currentVersion)
//            {
//                return this.Request.CreateResponse(HttpStatusCode.NotAcceptable);
//            }
//
//            if (deviceSyncProtocolVersion != serverSyncProtocolVersion)
//            {
//                return this.Request.CreateResponse(HttpStatusCode.NotAcceptable);
//            }

            return /*this.authorizedUser.DeviceId != deviceId
                ? this.Request.CreateResponse(HttpStatusCode.Forbidden)
                :*/ this.Request.CreateResponse(HttpStatusCode.OK, @"158329303");
        }

        private Version GetSupervisorVersionFromUserAgent(HttpRequestMessage request)
        {
            foreach (var product in request.Headers?.UserAgent)
            {
                if ((product.Product?.Name.Equals(@"org.worldbank.solutions.supervisor", StringComparison.OrdinalIgnoreCase) ?? false) 
                    && Version.TryParse(product.Product.Version, out Version version))
                {
                    return version;
                }
            }

            return null;
        }
    }
}
