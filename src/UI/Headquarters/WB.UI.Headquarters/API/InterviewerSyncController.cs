using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
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
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.UI.Headquarters.Controllers;
using WB.UI.Headquarters.Code;
using WB.UI.Headquarters.Resources;
using WB.UI.Shared.Web.Attributes;

namespace WB.UI.Headquarters.API
{
    public class InterviewerSyncController : BaseApiController
    { 
        private readonly ISyncProtocolVersionProvider syncVersionProvider;
        private readonly IAuthorizedUser authorizedUser;
        private readonly IImageFileStorage imageFileRepository;
        private readonly IFileSystemAccessor fileSystemAccessor; 
        private readonly ITabletInformationService tabletInformationService;
        private readonly IInterviewPackagesService incomingSyncPackagesQueue;
        
        private readonly IUserViewFactory userViewFactory;
        private readonly IAndroidPackageReader androidPackageReader;

        private string ResponseInterviewerFileName = "interviewer.apk";

        private string ResponseSupervisorFileName = "supervisor.apk";

        public InterviewerSyncController(ICommandService commandService,
            IAuthorizedUser authorizedUser,
            ILogger logger,
            IImageFileStorage imageFileRepository,
            IFileSystemAccessor fileSystemAccessor,
            IInterviewerSyncProtocolVersionProvider syncVersionProvider,
            ITabletInformationService tabletInformationService,
            IInterviewPackagesService incomingSyncPackagesQueue, 
            IUserViewFactory userViewFactory,
            IAndroidPackageReader androidPackageReader)
            : base(commandService, logger)
        {
            this.authorizedUser = authorizedUser;
            this.imageFileRepository = imageFileRepository;
            this.fileSystemAccessor = fileSystemAccessor;
            this.tabletInformationService = tabletInformationService;
            this.incomingSyncPackagesQueue = incomingSyncPackagesQueue;
            this.syncVersionProvider = syncVersionProvider;
            this.userViewFactory = userViewFactory;
            this.androidPackageReader = androidPackageReader;
        }

        [HttpGet]
        [ApiBasicAuth(new[] {UserRoles.Interviewer})]
        [Obsolete]
        public HttpResponseMessage GetHandshakePackage(string clientId, string androidId, Guid? clientRegistrationId, int version = 0)
        {
            int supervisorRevisionNumber = this.syncVersionProvider.GetProtocolVersion();

            this.Logger.Info(string.Format("Old version client. Client has protocol version {0} but current app protocol is {1} ", version, supervisorRevisionNumber));

            return this.Request.CreateErrorResponse(HttpStatusCode.NotAcceptable, TabletSyncMessages.InterviewerIsNotCompatibleWithThisVersion);
        }

        [HttpGet]
        [ApiBasicAuth(new[] {UserRoles.Interviewer})]
        public bool CheckExpectedDevice(string deviceId) =>
            string.IsNullOrEmpty(this.authorizedUser.DeviceId) || this.authorizedUser.DeviceId == deviceId;

        [HttpPost]
        [ApiBasicAuth(new[] { UserRoles.Interviewer })]
        public HttpResponseMessage GetHandshakePackage(HandshakePackageRequest request)
        {
            throw new HttpResponseException(new HttpResponseMessage(HttpStatusCode.NotAcceptable)
            {
                ReasonPhrase = TabletSyncMessages.InterviewerIsNotCompatibleWithThisVersion
            });
        }

        [HttpPost]
        [ApiBasicAuth(new[] { UserRoles.Interviewer })]
        public HttpResponseMessage PostFile(PostFileRequest request)
        {
            this.imageFileRepository.StoreInterviewBinaryData(request.InterviewId, request.FileName, Convert.FromBase64String(request.Data), null);
            return this.Request.CreateResponse(HttpStatusCode.OK);
        }

        [HttpGet]
        [AllowAnonymous]
        public HttpResponseMessage GetLatestVersion()
        {
            string pathToFile = this.fileSystemAccessor.CombinePath(HostingEnvironment.MapPath(ClientApkInfo.Directory), ClientApkInfo.InterviewerFileName);

            return this.CheckFileAndResponse(pathToFile, this.ResponseInterviewerFileName);
        }

        [HttpGet]
        [AllowAnonymous]
        public HttpResponseMessage GetLatestSupervisor()
        {
            string pathToFile = this.fileSystemAccessor.CombinePath(HostingEnvironment.MapPath(ClientApkInfo.Directory), ClientApkInfo.SupervisorFileName);

            return this.CheckFileAndResponse(pathToFile, ResponseSupervisorFileName);
        }

        private HttpResponseMessage CheckFileAndResponse(string pathToFile, string responseFileName)
        {
            if (!this.fileSystemAccessor.IsFileExists(pathToFile))
                return this.Request.CreateErrorResponse(HttpStatusCode.NotFound, TabletSyncMessages.FileWasNotFound);

            Stream fileStream = new FileStream(pathToFile, FileMode.Open, FileAccess.Read);
            var response = new ProgressiveDownload(this.Request).ResultMessage(fileStream,
                @"application/vnd.android.package-archive");

            response.Content.Headers.ContentDisposition = new ContentDispositionHeaderValue(@"attachment")
            {
                FileName = responseFileName
            };

            return response;
        }

        [HttpGet]
        [AllowAnonymous]
        public HttpResponseMessage GetLatestExtendedVersion()
        {
            string pathToFile = this.fileSystemAccessor.CombinePath(HostingEnvironment.MapPath(ClientApkInfo.Directory), ClientApkInfo.InterviewerExtendedFileName);

            return this.CheckFileAndResponse(pathToFile, this.ResponseInterviewerFileName);
        }

        [HttpGet]
        [AllowAnonymous]
        public bool CheckNewVersion(int versionCode)
        {
            string pathToInterviewerApp =
                this.fileSystemAccessor.CombinePath(HostingEnvironment.MapPath(ClientApkInfo.Directory), ClientApkInfo.InterviewerFileName);

            int? interviewerApkVersion = !this.fileSystemAccessor.IsFileExists(pathToInterviewerApp)
                ? null
                : this.androidPackageReader.Read(pathToInterviewerApp).Version;
            
            return interviewerApkVersion.HasValue && (interviewerApkVersion.Value > versionCode);
        }
    }
}
