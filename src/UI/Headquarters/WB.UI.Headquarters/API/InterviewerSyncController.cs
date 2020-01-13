using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Web.Hosting;
using System.Web.Http;
using Main.Core.Entities.SubEntities;
using WB.Core.BoundedContexts.Headquarters.Services;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.SharedKernel.Structures.Synchronization.SurveyManagement;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.UI.Headquarters.Controllers;
using WB.UI.Headquarters.Code;
using WB.UI.Headquarters.Resources;
using WB.UI.Headquarters.Services;
using WB.UI.Shared.Web.Extensions;

namespace WB.UI.Headquarters.API
{
    public class InterviewerSyncController : BaseApiController
    { 
        private readonly ISyncProtocolVersionProvider syncVersionProvider;
        private readonly IAuthorizedUser authorizedUser;
        private readonly IImageFileStorage imageFileRepository;
        private readonly IClientApkProvider clientApkProvider;

        public InterviewerSyncController(ICommandService commandService,
            IAuthorizedUser authorizedUser,
            ILogger logger,
            IImageFileStorage imageFileRepository,
            IInterviewerSyncProtocolVersionProvider syncVersionProvider,
            IClientApkProvider clientApkProvider)
            : base(commandService, logger)
        {
            this.authorizedUser = authorizedUser;
            this.imageFileRepository = imageFileRepository;
            this.syncVersionProvider = syncVersionProvider;
            this.clientApkProvider = clientApkProvider;
        }

        [HttpGet]
        [ApiBasicAuth(new[] {UserRoles.Interviewer})]
        [Obsolete]
        public HttpResponseMessage GetHandshakePackage(string clientId, string androidId, Guid? clientRegistrationId, int version = 0)
        {
            int supervisorRevisionNumber = this.syncVersionProvider.GetProtocolVersion();

            this.Logger.Info(
                $@"Old version client. Client has protocol version {version} " +
                $@"but current app protocol is {supervisorRevisionNumber} ");

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
            string pathToFile = Path.Combine(this.clientApkProvider.ApkClientsFolder(), ClientApkInfo.InterviewerFileName);

            return this.CheckFileAndResponse(pathToFile, ClientApkInfo.InterviewerResponseFileName);
        }

        [HttpGet]
        [AllowAnonymous]
        public HttpResponseMessage GetLatestSupervisor()
        {
            string pathToFile = Path.Combine(this.clientApkProvider.ApkClientsFolder(), ClientApkInfo.SupervisorFileName);

            return this.CheckFileAndResponse(pathToFile, ClientApkInfo.SupervisorFileName);
        }

        private HttpResponseMessage CheckFileAndResponse(string pathToFile, string responseFileName)
        {
            if (!System.IO.File.Exists(pathToFile))
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
            string pathToFile = Path.Combine(this.clientApkProvider.ApkClientsFolder(), ClientApkInfo.InterviewerExtendedFileName);

            return this.CheckFileAndResponse(pathToFile, ClientApkInfo.InterviewerResponseFileName);
        }

        [HttpGet]
        [AllowAnonymous]
        public bool CheckNewVersion(int versionCode)
        {
            var version = this.clientApkProvider.GetApplicationBuildNumber(ClientApkInfo.InterviewerFileName);
            return version.HasValue && (version.Value > versionCode);
        }
    }
}
