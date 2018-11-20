using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Web.Hosting;
using System.Web.Http;
using Main.Core.Entities.SubEntities;
using Microsoft.AspNet.Identity;
using WB.Core.BoundedContexts.Headquarters.Assignments;
using WB.Core.BoundedContexts.Headquarters.DataExport.Security;
using WB.Core.BoundedContexts.Headquarters.Factories;
using WB.Core.BoundedContexts.Headquarters.OwinSecurity;
using WB.Core.BoundedContexts.Headquarters.Services;
using WB.Core.BoundedContexts.Headquarters.Views;
using WB.Core.BoundedContexts.Headquarters.Views.SynchronizationLog;
using WB.Core.BoundedContexts.Headquarters.Views.User;
using WB.Core.Infrastructure.FileSystem;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.Infrastructure.Versions;
using WB.Core.SharedKernels.DataCollection;
using WB.UI.Headquarters.Code;
using WB.UI.Headquarters.Resources;
using WB.UI.Shared.Web.Filters;

namespace WB.UI.Headquarters.API.DataCollection.Interviewer
{
    public class InterviewerApiController : AppApiControllerBase
    {
        private const string RESPONSEAPPLICATIONFILENAME = "interviewer.apk";
        private const string PHYSICALAPPLICATIONFILENAME = "wbcapi.apk";
        private const string PHYSICALAPPLICATIONEXTENDEDFILENAME = "wbcapi.ext.apk";
        private const string PHYSICALPATHTOAPPLICATION = "~/Client/";
        
        private readonly IFileSystemAccessor fileSystemAccessor;
        protected readonly ITabletInformationService tabletInformationService;
        protected readonly IUserViewFactory userViewFactory;
        private readonly IAndroidPackageReader androidPackageReader;
        private readonly ISyncProtocolVersionProvider syncVersionProvider;
        private readonly IAuthorizedUser authorizedUser;
        private readonly IProductVersion productVersion;
        private readonly IAssignmentsService assignmentsService;
        private readonly HqSignInManager signInManager;
        private readonly IQuestionnaireBrowseViewFactory questionnaireBrowseViewFactory;

        public enum ClientVersionFromUserAgent
        {
            Unknown = 0,
            WithoutMaps = 1,
            WithMaps = 2
        }

        public InterviewerApiController(
            IFileSystemAccessor fileSystemAccessor,
            ITabletInformationService tabletInformationService,
            IUserViewFactory userViewFactory,
            IAndroidPackageReader androidPackageReader,
            IInterviewerSyncProtocolVersionProvider syncVersionProvider,
            IAuthorizedUser authorizedUser,
            IProductVersion productVersion,
            HqSignInManager signInManager,
            IQuestionnaireBrowseViewFactory questionnaireBrowseViewFactory,
            IAssignmentsService assignmentsService,
            IPlainKeyValueStorage<InterviewerSettings> interviewerSettingsStorage)
            : base(interviewerSettingsStorage)
        {
            this.fileSystemAccessor = fileSystemAccessor;
            this.tabletInformationService = tabletInformationService;
            this.userViewFactory = userViewFactory;
            this.androidPackageReader = androidPackageReader;
            this.syncVersionProvider = syncVersionProvider;
            this.authorizedUser = authorizedUser;
            this.productVersion = productVersion;
            this.signInManager = signInManager;
            this.questionnaireBrowseViewFactory = questionnaireBrowseViewFactory;
            this.assignmentsService = assignmentsService;
        }
        
        [HttpGet]
        [WriteToSyncLog(SynchronizationLogType.GetApk)]
        public virtual HttpResponseMessage Get()
        {
            var clientVersion = GetClientVersionFromUserAgent(this.Request);
            if (clientVersion == ClientVersionFromUserAgent.WithMaps)
                return this.HttpResponseMessage(PHYSICALAPPLICATIONEXTENDEDFILENAME, RESPONSEAPPLICATIONFILENAME);

            return this.HttpResponseMessage(PHYSICALAPPLICATIONFILENAME, RESPONSEAPPLICATIONFILENAME);
        }
        
        [HttpGet]
        [WriteToSyncLog(SynchronizationLogType.GetExtendedApk)]
        public virtual HttpResponseMessage GetExtended()
        {
            var clientVersion = GetClientVersionFromUserAgent(this.Request);
            if (clientVersion == ClientVersionFromUserAgent.WithoutMaps)
                return this.HttpResponseMessage(PHYSICALAPPLICATIONFILENAME, RESPONSEAPPLICATIONFILENAME);

            return this.HttpResponseMessage(PHYSICALAPPLICATIONEXTENDEDFILENAME, RESPONSEAPPLICATIONFILENAME);
        }

        private HttpResponseMessage HttpResponseMessage(string appName, string responseFileName)
        {
            string pathToInterviewerApp = this.fileSystemAccessor.CombinePath(HostingEnvironment.MapPath(PHYSICALPATHTOAPPLICATION), appName);

            if (!this.fileSystemAccessor.IsFileExists(pathToInterviewerApp))
                return this.Request.CreateErrorResponse(HttpStatusCode.NotFound, TabletSyncMessages.FileWasNotFound);

            Stream fileStream = new FileStream(pathToInterviewerApp, FileMode.Open, FileAccess.Read);
            var response = new ProgressiveDownload(this.Request).ResultMessage(fileStream,
                @"application/vnd.android.package-archive");

            response.Content.Headers.ContentDisposition = new ContentDispositionHeaderValue(@"attachment")
            {
                FileName = responseFileName 
            };

            return response;
        }

        [HttpGet]
        [WriteToSyncLog(SynchronizationLogType.GetApkPatch)]
        public virtual HttpResponseMessage Patch(int deviceVersion)
        {            
            var clientVersion = GetClientVersionFromUserAgent(this.Request);
            if(clientVersion == ClientVersionFromUserAgent.WithMaps)
                return GetPatchFile($@"WBCapi.{deviceVersion}.Ext.delta");

            return GetPatchFile($@"WBCapi.{deviceVersion}.delta");
        }

        [HttpGet]
        [WriteToSyncLog(SynchronizationLogType.GetExtendedApkPatch)]
        public virtual HttpResponseMessage PatchExtended(int deviceVersion)
        {
            var clientVersion = GetClientVersionFromUserAgent(this.Request);
            if (clientVersion == ClientVersionFromUserAgent.WithoutMaps)
                return GetPatchFile($@"WBCapi.{deviceVersion}.delta");

            return GetPatchFile($@"WBCapi.{deviceVersion}.Ext.delta");
        }

        private HttpResponseMessage GetPatchFile(string fileName)
        {
            string pathToInterviewerPatch = this.fileSystemAccessor.CombinePath(
                HostingEnvironment.MapPath(PHYSICALPATHTOAPPLICATION), fileName);

            if (!this.fileSystemAccessor.IsFileExists(pathToInterviewerPatch))
                return this.Request.CreateErrorResponse(HttpStatusCode.NotFound, TabletSyncMessages.FileWasNotFound);

            Stream fileStream = new FileStream(pathToInterviewerPatch, FileMode.Open, FileAccess.Read);
            return new ProgressiveDownload(this.Request).ResultMessage(fileStream, @"application/octet-stream");
        }

        [HttpGet]
        public virtual int? GetLatestVersion()
        {
            var clientVersion = GetClientVersionFromUserAgent(this.Request);
            if (clientVersion == ClientVersionFromUserAgent.WithMaps)
                return GetLatestVersion(PHYSICALAPPLICATIONEXTENDEDFILENAME);

            return GetLatestVersion(PHYSICALAPPLICATIONFILENAME);
        }

        [HttpGet]
        public virtual int? GetLatestExtendedVersion()
        {
            var clientVersion = GetClientVersionFromUserAgent(this.Request);
            if (clientVersion == ClientVersionFromUserAgent.WithoutMaps)
                return GetLatestVersion(PHYSICALAPPLICATIONFILENAME);

            return GetLatestVersion(PHYSICALAPPLICATIONEXTENDEDFILENAME);
        }

        private int? GetLatestVersion(string appName)
        {
            string pathToInterviewerApp = this.fileSystemAccessor.CombinePath(HostingEnvironment.MapPath(PHYSICALPATHTOAPPLICATION),
                appName);

            return !this.fileSystemAccessor.IsFileExists(pathToInterviewerApp)
                ? null
                : this.androidPackageReader.Read(pathToInterviewerApp).Version;
        }

        [HttpPost]
        public virtual async Task<HttpResponseMessage> PostTabletInformation()
        {
            HttpRequestMessage request = this.Request;
            if (!request.Content.IsMimeMultipartContent())
            {
                throw new HttpResponseException(HttpStatusCode.UnsupportedMediaType);
            }

            var authHeader = request.Headers.Authorization?.ToString();

            if (authHeader != null)
            {
                await signInManager.SignInWithAuthTokenAsync(authHeader, false, UserRoles.Interviewer);
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

        [ApiBasicAuth(UserRoles.Interviewer)]
        [WriteToSyncLog(SynchronizationLogType.CanSynchronize)]
        [HttpGet]
        [ApiNoCache]
        public virtual HttpResponseMessage CheckCompatibility(string deviceId, int deviceSyncProtocolVersion)
        {
            int serverSyncProtocolVersion = this.syncVersionProvider.GetProtocolVersion();
            int lastNonUpdatableSyncProtocolVersion = this.syncVersionProvider.GetLastNonUpdatableVersion();

            if (deviceSyncProtocolVersion < lastNonUpdatableSyncProtocolVersion)
                return this.Request.CreateResponse(HttpStatusCode.UpgradeRequired);

            var currentVersion = new Version(this.productVersion.ToString().Split(' ')[0]);
            var interviewerVersion = GetInterviewerVersionFromUserAgent(this.Request);

            if (IsNeedUpdateAppBySettings(interviewerVersion, currentVersion))
            {
                return this.Request.CreateResponse(HttpStatusCode.UpgradeRequired);
            }

            if (interviewerVersion != null && interviewerVersion > currentVersion)
            {
                return this.Request.CreateResponse(HttpStatusCode.NotAcceptable);
            }

            if (deviceSyncProtocolVersion == 7070) // KP-11462
            {
                return this.Request.CreateResponse(HttpStatusCode.UpgradeRequired);
            }

            if (deviceSyncProtocolVersion == 7060 /* pre protected questions release */)
            {
                if (deviceSyncProtocolVersion < InterviewerSyncProtocolVersionProvider.ProtectedVariablesIntroduced
                    && this.assignmentsService.HasAssignmentWithProtectedVariables(this.authorizedUser.Id))
                {
                    return this.Request.CreateResponse(HttpStatusCode.UpgradeRequired);
                }
            }
            else if (deviceSyncProtocolVersion == 7050 /* PRE assignment devices, that still allowed to connect*/)
            {
                var interviewerAssignments = this.assignmentsService.GetAssignments(this.authorizedUser.Id);
                var assignedQuestionarries = this.questionnaireBrowseViewFactory.GetByIds(interviewerAssignments.Select(ia => ia.QuestionnaireId).ToArray());

                if (assignedQuestionarries.Any(aq => aq.AllowAssignments))
                {
                    return this.Request.CreateResponse(HttpStatusCode.UpgradeRequired);
                }

            }
            else if (deviceSyncProtocolVersion != serverSyncProtocolVersion)
            {
                return this.Request.CreateResponse(HttpStatusCode.NotAcceptable);
            }

            return this.authorizedUser.DeviceId != deviceId
                ? this.Request.CreateResponse(HttpStatusCode.Forbidden)
                : this.Request.CreateResponse(HttpStatusCode.OK, @"449634775");
        }

        private Version GetInterviewerVersionFromUserAgent(HttpRequestMessage request)
        {
            foreach (var product in request.Headers?.UserAgent)
            {
                if ((product.Product?.Name.Equals(@"org.worldbank.solutions.interviewer", StringComparison.OrdinalIgnoreCase)
                    ?? false) && Version.TryParse(product.Product.Version, out Version version))
                {
                    return version;
                }
            }

            return null;
        }

        private ClientVersionFromUserAgent GetClientVersionFromUserAgent(HttpRequestMessage request)
        {
            if (request.Headers?.UserAgent != null)
            {
                foreach (var product in request.Headers?.UserAgent)
                {
                    if (product.Product?.Name.Equals(@"maps",StringComparison.OrdinalIgnoreCase)??false)
                    {
                        return ClientVersionFromUserAgent.WithMaps;
                    }
                }
                return ClientVersionFromUserAgent.WithoutMaps;
            }
            return ClientVersionFromUserAgent.Unknown;
        }
    }
}
