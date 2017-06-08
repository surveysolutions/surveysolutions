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
using WB.Core.BoundedContexts.Headquarters.Factories;
using WB.Core.BoundedContexts.Headquarters.OwinSecurity;
using WB.Core.BoundedContexts.Headquarters.Services;
using WB.Core.BoundedContexts.Headquarters.Views.SynchronizationLog;
using WB.Core.BoundedContexts.Headquarters.Views.User;
using WB.Core.Infrastructure.FileSystem;
using WB.Core.Infrastructure.Versions;
using WB.Core.SharedKernels.DataCollection;
using WB.UI.Headquarters.Code;
using WB.UI.Headquarters.Resources;

namespace WB.UI.Headquarters.API.Interviewer
{
    public class InterviewerApiController : ApiController
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

        public InterviewerApiController(
            IFileSystemAccessor fileSystemAccessor,
            ITabletInformationService tabletInformationService,
            IUserViewFactory userViewFactory,
            IAndroidPackageReader androidPackageReader,
            ISyncProtocolVersionProvider syncVersionProvider,
            IAuthorizedUser authorizedUser,
            IProductVersion productVersion,
            HqSignInManager signInManager,
            IQuestionnaireBrowseViewFactory questionnaireBrowseViewFactory,
            IAssignmentsService assignmentsService)
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
        public virtual HttpResponseMessage Get()
        {
            string pathToInterviewerApp = this.fileSystemAccessor.CombinePath(HostingEnvironment.MapPath(PHYSICALPATHTOAPPLICATION), PHYSICALAPPLICATIONFILENAME);

            return this.HttpResponseMessage(pathToInterviewerApp);
        }

        [HttpGet]
        public virtual HttpResponseMessage GetExtended()
        {
            string pathToInterviewerApp = this.fileSystemAccessor.CombinePath(HostingEnvironment.MapPath(PHYSICALPATHTOAPPLICATION), PHYSICALAPPLICATIONEXTENDEDFILENAME);

            return this.HttpResponseMessage(pathToInterviewerApp);
        }

        private HttpResponseMessage HttpResponseMessage(string pathToInterviewerApp)
        {
            if (!this.fileSystemAccessor.IsFileExists(pathToInterviewerApp))
                return this.Request.CreateErrorResponse(HttpStatusCode.NotFound, TabletSyncMessages.FileWasNotFound);

            Stream fileStream = new FileStream(pathToInterviewerApp, FileMode.Open, FileAccess.Read);
            var response = new ProgressiveDownload(this.Request).ResultMessage(fileStream,
                @"application/vnd.android.package-archive");

            response.Content.Headers.ContentDisposition = new ContentDispositionHeaderValue(@"attachment")
            {
                FileName = RESPONSEAPPLICATIONFILENAME
            };

            return response;
        }

        [HttpGet]
        public virtual HttpResponseMessage Patch(int deviceVersion)
        {
            string pathToInterviewerPatch = this.fileSystemAccessor.CombinePath(
                HostingEnvironment.MapPath(PHYSICALPATHTOAPPLICATION), $@"WBCapi.{deviceVersion}.delta");

            if (!this.fileSystemAccessor.IsFileExists(pathToInterviewerPatch))
                return this.Request.CreateErrorResponse(HttpStatusCode.NotFound, TabletSyncMessages.FileWasNotFound);

            Stream fileStream = new FileStream(pathToInterviewerPatch, FileMode.Open, FileAccess.Read);
            return new ProgressiveDownload(this.Request).ResultMessage(fileStream, @"application/octet-stream");
        }

        [HttpGet]
        public virtual HttpResponseMessage PatchExtended(int deviceVersion)
        {
            string pathToInterviewerPatch = this.fileSystemAccessor.CombinePath(
                HostingEnvironment.MapPath(PHYSICALPATHTOAPPLICATION), $@"WBCapi.Ext.{deviceVersion}.delta");

            if (!this.fileSystemAccessor.IsFileExists(pathToInterviewerPatch))
                return this.Request.CreateErrorResponse(HttpStatusCode.NotFound, TabletSyncMessages.FileWasNotFound);

            Stream fileStream = new FileStream(pathToInterviewerPatch, FileMode.Open, FileAccess.Read);
            return new ProgressiveDownload(this.Request).ResultMessage(fileStream, @"application/octet-stream");
        }

        [HttpGet]
        public virtual int? GetLatestVersion()
        {
            string pathToInterviewerApp = this.fileSystemAccessor.CombinePath(HostingEnvironment.MapPath(PHYSICALPATHTOAPPLICATION),
                    PHYSICALAPPLICATIONFILENAME);

            return !this.fileSystemAccessor.IsFileExists(pathToInterviewerApp)
                ? null
                : this.androidPackageReader.Read(pathToInterviewerApp).Version;
        }

        [HttpGet]
        public virtual int? GetLatestExtendedVersion()
        {
            string pathToInterviewerApp = this.fileSystemAccessor.CombinePath(HostingEnvironment.MapPath(PHYSICALPATHTOAPPLICATION),
                    PHYSICALAPPLICATIONEXTENDEDFILENAME);

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
        public virtual HttpResponseMessage CheckCompatibility(string deviceId, int deviceSyncProtocolVersion)
        {
            int serverSyncProtocolVersion = this.syncVersionProvider.GetProtocolVersion();
            int lastNonUpdatableSyncProtocolVersion = this.syncVersionProvider.GetLastNonUpdatableVersion();

            if (deviceSyncProtocolVersion < lastNonUpdatableSyncProtocolVersion)
                return this.Request.CreateResponse(HttpStatusCode.UpgradeRequired);

            var currentVersion = new Version(this.productVersion.ToString().Split(' ')[0]);
            var interviewerVersion = GetInterviewerVersionFromUserAgent(this.Request);

            if (interviewerVersion != null && interviewerVersion > currentVersion)
            {
                return this.Request.CreateResponse(HttpStatusCode.NotAcceptable);
            }

            if (deviceSyncProtocolVersion == 7050 /* PRE assignment devices, that still allowed to connect*/)
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
    }
}