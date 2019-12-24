using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Hosting;
using System.Web.Http;
using Main.Core.Entities.SubEntities;
using Microsoft.AspNet.Identity;
using WB.Core.BoundedContexts.Headquarters.DataExport.Security;
using WB.Core.BoundedContexts.Headquarters.Implementation;
using WB.Core.BoundedContexts.Headquarters.OwinSecurity;
using WB.Core.BoundedContexts.Headquarters.Services;
using WB.Core.BoundedContexts.Headquarters.Views.Interview;
using WB.Core.BoundedContexts.Headquarters.Views.User;
using WB.Core.BoundedContexts.Headquarters.Views.SynchronizationLog;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.Infrastructure.Versions;
using WB.Core.SharedKernels.DataCollection;
using WB.UI.Headquarters.Code;
using WB.UI.Headquarters.Services;
using WB.UI.Headquarters.Utils;
using WB.UI.Shared.Web.Extensions;
using WB.UI.Shared.Web.Filters;

namespace WB.UI.Headquarters.API.DataCollection.Supervisor.v1
{
    public class SupervisorApiController : AppApiControllerBase
    {
        private readonly ITabletInformationService tabletInformationService;
        private readonly HqSignInManager signInManager;
        private readonly ISupervisorSyncProtocolVersionProvider syncVersionProvider;
        private readonly IProductVersion productVersion;
        private readonly IUserViewFactory userViewFactory;
        private readonly IClientApkProvider clientApkProvider;
        private readonly IAuthorizedUser authorizedUser;
        private readonly IInterviewInformationFactory interviewFactory;

        public SupervisorApiController(ITabletInformationService tabletInformationService, 
            ISupervisorSyncProtocolVersionProvider syncVersionProvider,
            IProductVersion productVersion,
            IUserViewFactory userViewFactory, 
            HqSignInManager signInManager,
            IPlainKeyValueStorage<InterviewerSettings> settingsStorage,
            IPlainKeyValueStorage<TenantSettings> tenantSettings,
            IClientApkProvider clientApkProvider,
            IAuthorizedUser authorizedUser,
            IInterviewInformationFactory interviewFactory)
            : base(settingsStorage, tenantSettings)
        {
            this.tabletInformationService = tabletInformationService;
            this.syncVersionProvider = syncVersionProvider;
            this.productVersion = productVersion;
            this.userViewFactory = userViewFactory;
            this.signInManager = signInManager;
            this.clientApkProvider = clientApkProvider;
            this.authorizedUser = authorizedUser;
            this.interviewFactory = interviewFactory;
        }

        [HttpGet]
        [WriteToSyncLog(SynchronizationLogType.GetSupervisorApk)]
        public virtual HttpResponseMessage GetSupervisor() =>
            this.clientApkProvider.GetApkAsHttpResponse(Request, ClientApkInfo.SupervisorFileName, ClientApkInfo.SupervisorFileName);

        [HttpGet]
        [WriteToSyncLog(SynchronizationLogType.GetApk)]
        public virtual HttpResponseMessage GetInterviewer() =>
            this.clientApkProvider.GetApkAsHttpResponse(Request, ClientApkInfo.InterviewerFileName, ClientApkInfo.InterviewerFileName);

        [HttpGet]
        [WriteToSyncLog(SynchronizationLogType.GetExtendedApk)]
        public virtual HttpResponseMessage GetInterviewerWithMaps() =>
            this.clientApkProvider.GetApkAsHttpResponse(Request, ClientApkInfo.InterviewerExtendedFileName, ClientApkInfo.InterviewerFileName);
        
        [HttpGet]
        public virtual int? GetLatestVersion()
        {
            return this.clientApkProvider.GetLatestVersion(ClientApkInfo.SupervisorFileName);
        }

        [ApiBasicAuth(UserRoles.Supervisor)]
        [WriteToSyncLog(SynchronizationLogType.CanSynchronize)]
        [HttpGet]
        [ApiNoCache]
        public virtual HttpResponseMessage CheckCompatibility(string deviceId, int deviceSyncProtocolVersion, string tenantId = null)
        {
            int serverSyncProtocolVersion = this.syncVersionProvider.GetProtocolVersion();
            int lastNonUpdatableSyncProtocolVersion = this.syncVersionProvider.GetLastNonUpdatableVersion();
            if (deviceSyncProtocolVersion < lastNonUpdatableSyncProtocolVersion)
                return this.Request.CreateResponse(HttpStatusCode.UpgradeRequired);

            if (!UserIsFromThisTenant(tenantId))
            {
                return this.Request.CreateResponse(HttpStatusCode.Conflict);
            }
            
            if (deviceSyncProtocolVersion < SupervisorSyncProtocolVersionProvider.V2_ResolvedCommentsIntroduced)
            {
                if (this.interviewFactory.HasAnyInterviewsInProgressWithResolvedCommentsForSupervisor(
                    this.authorizedUser.Id))
                {
                    return this.Request.CreateResponse(HttpStatusCode.UpgradeRequired);
                }
            }

            var currentVersion = new Version(this.productVersion.ToString().Split(' ')[0]);
            var supervisorVersion = this.Request.GetProductVersionFromUserAgent(@"org.worldbank.solutions.supervisor");

            if (IsNeedUpdateAppBySettings(supervisorVersion, currentVersion))
            {
                return this.Request.CreateResponse(HttpStatusCode.UpgradeRequired);
            }

            if (supervisorVersion != null && supervisorVersion > currentVersion)
            {
                return this.Request.CreateResponse(HttpStatusCode.NotAcceptable);
            }

            if (deviceSyncProtocolVersion == SupervisorSyncProtocolVersionProvider.V1_BeforeResolvedCommentsIntroduced) 
            {
                // allowed to synchronize
            }
            else if (deviceSyncProtocolVersion != serverSyncProtocolVersion)
            {
                return this.Request.CreateResponse(HttpStatusCode.NotAcceptable);
            }

            return this.Request.CreateResponse(HttpStatusCode.OK, @"158329303");
        }

        [HttpPost]
        public async Task<HttpResponseMessage> PostTabletInformation()
        {
            HttpRequestMessage request = this.Request;
            if (!request.Content.IsMimeMultipartContent())
            {
                throw new HttpResponseException(HttpStatusCode.UnsupportedMediaType);
            }

            var authHeader = request.Headers.Authorization?.ToString();

            if (authHeader != null)
            {
                await signInManager.SignInWithAuthTokenAsync(authHeader, false, UserRoles.Supervisor);
            }

            var multipartMemoryStreamProvider = await request.Content.ReadAsMultipartAsync();
            var httpContent = multipartMemoryStreamProvider.Contents.Single();
            var fileContent = await httpContent.ReadAsByteArrayAsync();

            var deviceId = this.Request.Headers.GetValues(@"DeviceId").Single();
            var userId = User.Identity.GetUserId();

            var user = userId != null
                ? this.userViewFactory.GetUser(new UserViewInputModel(Guid.Parse(userId)))
                : null;

            this.tabletInformationService.SaveTabletInformation(
                content: fileContent,
                androidId: deviceId,
                user: user);

            return this.Request.CreateResponse(HttpStatusCode.OK);
        }
     
        [HttpGet]
        [WriteToSyncLog(SynchronizationLogType.GetSupervisorApkPatch)]
        public virtual HttpResponseMessage Patch(int deviceVersion)
        {
            return this.clientApkProvider.GetPatchFileAsHttpResponse(Request, $@"Supervisor.{deviceVersion}.delta");
        }
    }
}
