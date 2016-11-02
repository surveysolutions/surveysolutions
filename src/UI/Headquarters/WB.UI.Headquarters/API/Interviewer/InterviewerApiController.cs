using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Web.Hosting;
using System.Web.Http;
using Main.Core.Entities.SubEntities;
using WB.Core.BoundedContexts.Headquarters.Services;
using WB.Core.BoundedContexts.Headquarters.Views.SynchronizationLog;
using WB.Core.BoundedContexts.Headquarters.Views.User;
using WB.Core.Infrastructure.FileSystem;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.SurveyManagement.Web.Code;
using WB.Core.SharedKernels.SurveyManagement.Web.Models.User;
using WB.Core.SharedKernels.SurveyManagement.Web.Utils.Membership;
using WB.UI.Headquarters.Code;
using WB.UI.Headquarters.Resources;

namespace WB.UI.Headquarters.API.Interviewer
{
    public class InterviewerApiController : ApiController
    {
        private const string RESPONSEAPPLICATIONFILENAME = "interviewer.apk";
        private const string PHYSICALAPPLICATIONFILENAME = "wbcapi.apk";
        private const string PHYSICALPATHTOAPPLICATION = "~/Client/";

        private readonly IFileSystemAccessor fileSystemAccessor;
        protected readonly ITabletInformationService tabletInformationService;
        protected readonly IUserViewFactory userViewFactory;
        private readonly IAndroidPackageReader androidPackageReader;
        private readonly IUserWebViewFactory userInfoViewFactory;
        private readonly ISyncProtocolVersionProvider syncVersionProvider;
        private readonly IGlobalInfoProvider globalInfoProvider;

        public InterviewerApiController(
            IFileSystemAccessor fileSystemAccessor,
            ITabletInformationService tabletInformationService,
            IUserViewFactory userViewFactory,
            IAndroidPackageReader androidPackageReader,
            IUserWebViewFactory userInfoViewFactory,
            ISyncProtocolVersionProvider syncVersionProvider,
            IGlobalInfoProvider globalInfoProvider)
        {
            this.fileSystemAccessor = fileSystemAccessor;
            this.tabletInformationService = tabletInformationService;
            this.userViewFactory = userViewFactory;
            this.androidPackageReader = androidPackageReader;
            this.userInfoViewFactory = userInfoViewFactory;
            this.syncVersionProvider = syncVersionProvider;
            this.globalInfoProvider = globalInfoProvider;
        }

        [HttpGet]
        public virtual HttpResponseMessage Get()
        {
            string pathToInterviewerApp = this.fileSystemAccessor.CombinePath(HostingEnvironment.MapPath(PHYSICALPATHTOAPPLICATION), PHYSICALAPPLICATIONFILENAME);

            if (!this.fileSystemAccessor.IsFileExists(pathToInterviewerApp))
                return this.Request.CreateErrorResponse(HttpStatusCode.NotFound, TabletSyncMessages.FileWasNotFound);

            var response = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StreamContent(this.fileSystemAccessor.ReadFile(pathToInterviewerApp))
            };

            response.Content.Headers.ContentType = new MediaTypeHeaderValue("application/vnd.android.package-archive");
            response.Content.Headers.ContentDisposition = new ContentDispositionHeaderValue("attachment")
            {
                FileName = RESPONSEAPPLICATIONFILENAME
            };

            return response;
        }

        [HttpGet]
        public virtual int? GetLatestVersion()
        {
            string pathToInterviewerApp =
                this.fileSystemAccessor.CombinePath(HostingEnvironment.MapPath(PHYSICALPATHTOAPPLICATION),
                    PHYSICALAPPLICATIONFILENAME);

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

            var multipartMemoryStreamProvider = await request.Content.ReadAsMultipartAsync();
            var httpContent = multipartMemoryStreamProvider.Contents.Single();
            var fileContent = await httpContent.ReadAsByteArrayAsync();

            var deviceId = this.Request.Headers.GetValues("DeviceId").Single();
            var user = this.userViewFactory.Load(new UserViewInputModel(deviceId));

            this.tabletInformationService.SaveTabletInformation(
                content: fileContent,
                androidId: deviceId,
                user: user);

            return this.Request.CreateResponse(HttpStatusCode.OK);
        }

        [ApiBasicAuth(new[] { UserRoles.Operator })]
        [WriteToSyncLog(SynchronizationLogType.CanSynchronize)]
        [HttpGet]
        public virtual HttpResponseMessage CheckCompatibility(string deviceId, int deviceSyncProtocolVersion)
        {
            int serverSyncProtocolVersion = this.syncVersionProvider.GetProtocolVersion();
            int lastNonUpdatableSyncProtocolVersion = this.syncVersionProvider.GetLastNonUpdatableVersion();

            if (deviceSyncProtocolVersion < lastNonUpdatableSyncProtocolVersion)
                return this.Request.CreateResponse(HttpStatusCode.UpgradeRequired);

            if (deviceSyncProtocolVersion != serverSyncProtocolVersion)
                return this.Request.CreateResponse(HttpStatusCode.NotAcceptable);


            var interviewerInfo = this.userInfoViewFactory.Load(new UserWebViewInputModel(this.globalInfoProvider.GetCurrentUser().Name, null));
            return interviewerInfo.DeviceId != deviceId
                ? this.Request.CreateResponse(HttpStatusCode.Forbidden)
                : this.Request.CreateResponse(HttpStatusCode.OK, "449634775");
        }
    }
}