using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Web.Hosting;
using System.Web.Http;
using Main.Core.Entities.SubEntities;
using WB.Core.BoundedContexts.Headquarters.Services;
using WB.Core.BoundedContexts.Headquarters.Views.User;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.Infrastructure.FileSystem;
using WB.Core.SharedKernel.Structures.Synchronization.SurveyManagement;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernel.Structures.TabletInformation;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.SurveyManagement.Web.Code;
using WB.Core.SharedKernels.SurveyManagement.Web.Controllers;
using WB.Core.SharedKernels.SurveyManagement.Web.Models.User;
using WB.Core.SharedKernels.SurveyManagement.Web.Utils.Membership;
using WB.Core.Synchronization;
using WB.UI.Headquarters.Resources;

namespace WB.Core.SharedKernels.SurveyManagement.Web.Api
{
    public class InterviewerSyncController : BaseApiController
    { 
        private readonly ISyncProtocolVersionProvider syncVersionProvider;
        private readonly IPlainInterviewFileStorage plainFileRepository;
        private readonly IFileSystemAccessor fileSystemAccessor; 
        private readonly ITabletInformationService tabletInformationService;
        private readonly IInterviewPackagesService incomingSyncPackagesQueue;

        private readonly IUserWebViewFactory userInfoViewFactory;
        private readonly IUserViewFactory userViewFactory;
        private readonly IAndroidPackageReader androidPackageReader;

        private string ResponseInterviewerFileName = "interviewer.apk";
        private string CapiFileName = "wbcapi.apk";
        private string pathToSearchVersions = ("~/Client/");


        public InterviewerSyncController(ICommandService commandService,
            IGlobalInfoProvider globalInfo,
            ILogger logger,
            IPlainInterviewFileStorage plainFileRepository,
            IFileSystemAccessor fileSystemAccessor,
            ISyncProtocolVersionProvider syncVersionProvider,
            ITabletInformationService tabletInformationService,
            IInterviewPackagesService incomingSyncPackagesQueue, 
            IUserWebViewFactory userInfoViewFactory,
            IUserViewFactory userViewFactory,
            IAndroidPackageReader androidPackageReader)
            : base(commandService, globalInfo, logger)
        {

            this.plainFileRepository = plainFileRepository;
            this.fileSystemAccessor = fileSystemAccessor;
            this.tabletInformationService = tabletInformationService;
            this.incomingSyncPackagesQueue = incomingSyncPackagesQueue;
            this.userInfoViewFactory = userInfoViewFactory;
            this.syncVersionProvider = syncVersionProvider;
            this.userViewFactory = userViewFactory;
            this.androidPackageReader = androidPackageReader;
        }

        [HttpGet]
        [ApiBasicAuth(new[] {UserRoles.Operator})]
        [Obsolete]
        public HttpResponseMessage GetHandshakePackage(string clientId, string androidId, Guid? clientRegistrationId, int version = 0)
        {
            int supervisorRevisionNumber = syncVersionProvider.GetProtocolVersion();

            Logger.Info(string.Format("Old version client. Client has protocol version {0} but current app protocol is {1} ", version, supervisorRevisionNumber));

            return Request.CreateErrorResponse(HttpStatusCode.NotAcceptable, TabletSyncMessages.InterviewerIsNotCompatibleWithThisVersion);
        }

        [HttpGet]
        [ApiBasicAuth(new[] { UserRoles.Operator })]
        public bool CheckExpectedDevice(string deviceId)
        {
            var interviewerInfo = userInfoViewFactory.Load(new UserWebViewInputModel(this.GlobalInfo.GetCurrentUser().Name, null));
            return string.IsNullOrEmpty(interviewerInfo.DeviceId) || interviewerInfo.DeviceId == deviceId;
        }

        [HttpPost]
        [ApiBasicAuth(new[] { UserRoles.Operator })]
        public HttpResponseMessage GetHandshakePackage(HandshakePackageRequest request)
        {
            throw new HttpResponseException(new HttpResponseMessage(HttpStatusCode.NotAcceptable)
            {
                ReasonPhrase = TabletSyncMessages.InterviewerIsNotCompatibleWithThisVersion
            });
        }

        [HttpPost]
        [ApiBasicAuth(new[] { UserRoles.Operator })]
        public HttpResponseMessage PostFile(PostFileRequest request)
        {
            plainFileRepository.StoreInterviewBinaryData(request.InterviewId, request.FileName, Convert.FromBase64String(request.Data));
            return Request.CreateResponse(HttpStatusCode.OK);
        }

        [HttpPost]
        [ApiBasicAuth(new[] { UserRoles.Operator })]
        public (PostPackageRequest request)
        {
            this.incomingSyncPackagesQueue.StoreOrProcessPackage(item: request.SynchronizationPackage);

            return Request.CreateResponse(HttpStatusCode.OK);
        }

        [HttpGet]
        [AllowAnonymous]
        public HttpResponseMessage GetLatestVersion()
        {
            string pathToFile = fileSystemAccessor.CombinePath(HostingEnvironment.MapPath(pathToSearchVersions), CapiFileName);

            if (fileSystemAccessor.IsFileExists(pathToFile))
            {
                var response = new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StreamContent(fileSystemAccessor.ReadFile(pathToFile))
                };

                response.Content.Headers.ContentType = new MediaTypeHeaderValue("application/vnd.android.package-archive");
                response.Content.Headers.ContentDisposition = new ContentDispositionHeaderValue("attachment")
                {
                    FileName = ResponseInterviewerFileName
                };

                return response;
            }

            return Request.CreateErrorResponse(HttpStatusCode.NotFound, TabletSyncMessages.FileWasNotFound);
        }

        [HttpGet]
        [AllowAnonymous]
        public bool CheckNewVersion(int versionCode)
        {
            string pathToInterviewerApp =
                this.fileSystemAccessor.CombinePath(HostingEnvironment.MapPath(pathToSearchVersions), CapiFileName);

            int? interviewerApkVersion = !this.fileSystemAccessor.IsFileExists(pathToInterviewerApp)
                ? null
                : this.androidPackageReader.Read(pathToInterviewerApp).Version;
            
            return interviewerApkVersion.HasValue && (interviewerApkVersion.Value > versionCode);
        }

        [HttpPost]
        [AllowAnonymous]
        public void PostInfoPackage(TabletInformationPackage tabletInformationPackage)
        {
            var user = this.userViewFactory.Load(new UserViewInputModel(tabletInformationPackage.AndroidId));

            this.tabletInformationService.SaveTabletInformation(
                content: Convert.FromBase64String(tabletInformationPackage.Content),
                androidId: tabletInformationPackage.AndroidId,
                user: user);

            //log record
        }
    }
}