using System;
using System.Globalization;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Web.Hosting;
using System.Web.Http;
using WB.Core.GenericSubdomains.Utils.Services;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.Infrastructure.FileSystem;
using WB.Core.SharedKernel.Structures.Synchronization;
using WB.Core.SharedKernel.Structures.Synchronization.SurveyManagement;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernel.Structures.TabletInformation;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.SurveyManagement.Services;
using WB.Core.SharedKernels.SurveyManagement.Web.Code;
using WB.Core.SharedKernels.SurveyManagement.Web.Controllers;
using WB.Core.SharedKernels.SurveyManagement.Web.Models.User;
using WB.Core.SharedKernels.SurveyManagement.Web.Resources;
using WB.Core.SharedKernels.SurveyManagement.Web.Utils.Membership;
using WB.Core.Synchronization;
using WB.Core.Synchronization.SyncStorage;

namespace WB.Core.SharedKernels.SurveyManagement.Web.Api
{
    public class InterviewerSyncController : BaseApiController
    {
        private readonly ISyncManager syncManager;
        private readonly IUserWebViewFactory userInfoViewFactory;
        private readonly ISupportedVersionProvider versionProvider;
        private readonly ISyncProtocolVersionProvider syncVersionProvider;
        private readonly IPlainInterviewFileStorage plainFileRepository;
        private readonly IFileSystemAccessor fileSystemAccessor; 
        private readonly ITabletInformationService tabletInformationService;

        private string ResponseInterviewerFileName = "interviewer.apk";
        private string CapiFileName = "wbcapi.apk";
        private string pathToSearchVersions = ("~/Client/");


        public InterviewerSyncController(ICommandService commandService,
            IGlobalInfoProvider globalInfo,
            ISyncManager syncManager,
            ILogger logger,
            IUserWebViewFactory userInfoViewFactory,
            ISupportedVersionProvider versionProvider,
            IPlainInterviewFileStorage plainFileRepository,
            IFileSystemAccessor fileSystemAccessor,
            ISyncProtocolVersionProvider syncVersionProvider,
            ITabletInformationService tabletInformationService)
            : base(commandService, globalInfo, logger)
        {

            this.plainFileRepository = plainFileRepository;
            this.versionProvider = versionProvider;
            this.syncManager = syncManager;
            this.userInfoViewFactory = userInfoViewFactory;
            this.fileSystemAccessor = fileSystemAccessor;
            this.tabletInformationService = tabletInformationService;
            this.syncVersionProvider = syncVersionProvider;
        }

        [HttpGet]
        [ApiBasicAuth]
        [Obsolete]
        public HttpResponseMessage GetHandshakePackage(string clientId, string androidId, Guid? clientRegistrationId, int version = 0)
        {
            int supervisorRevisionNumber = syncVersionProvider.GetProtocolVersion();

            Logger.Info(string.Format("Old version client. Client has protocol version {0} but current app protocol is {1} ", version, supervisorRevisionNumber));

            return Request.CreateErrorResponse(HttpStatusCode.NotAcceptable, InterviewerSyncStrings.InterviewerIsNotCompatibleWithThisVersion);
        }

        private static HttpResponseException CreateRestException(HttpStatusCode httpStatusCode, string message)
        {
            return new HttpResponseException(new HttpResponseMessage(httpStatusCode) {ReasonPhrase = message});
        }


        [HttpPost]
        [ApiBasicAuth]
        public HandshakePackage GetHandshakePackage(HandshakePackageRequest request)
        {
            int supervisorRevisionNumber = syncVersionProvider.GetProtocolVersion();

            int supervisorShiftVersionNumber = syncVersionProvider.GetLastNonUpdatableVersion();
            
            if (request.Version > supervisorRevisionNumber)
            {
                Logger.Info(string.Format("Version mismatch. Client from the future. Client has protocol version {0} but current app protocol is {1} ", request.Version, supervisorRevisionNumber));

                throw CreateRestException(HttpStatusCode.NotAcceptable, string.Format(InterviewerSyncStrings.InterviewerApplicationHasHigherVersion_thanSupervisor_Format, request.Version, supervisorRevisionNumber));
            }

            if (request.Version < supervisorShiftVersionNumber)
            {
                Logger.Info(string.Format(" Client has protocol version {0} but current app protocol is {1}. Major change.", request.Version, supervisorRevisionNumber));

                throw CreateRestException(HttpStatusCode.NotAcceptable, InterviewerSyncStrings.InterviewerApplicationHasVersion_butSupervisorHas_PleaseReinstallInterviewerApplication);
            }

            if (request.Version < supervisorRevisionNumber)
            {
                Logger.Info(string.Format(" Client has protocol version {0} but current app protocol is {1} ", request.Version, supervisorRevisionNumber));

                throw CreateRestException(HttpStatusCode.NotAcceptable,
                    string.Format(InterviewerSyncStrings.InterviewerApplicationHasVersion_butSupervisorHas_PleaseUpdateInterviewerApplication, request.Version, supervisorRevisionNumber));
            }

            if (string.IsNullOrEmpty(request.AndroidId))
            {
                Logger.Info(string.Format("Android device id was not provided"));

                throw CreateRestException(HttpStatusCode.NotAcceptable, InterviewerSyncStrings.AndroidDeviceIdWasNotProvided);
            }

            var interviewerInfo = userInfoViewFactory.Load(new UserWebViewInputModel(this.GlobalInfo.GetCurrentUser().Name, null));

            if (!string.IsNullOrEmpty(interviewerInfo.DeviceId) && interviewerInfo.DeviceId != request.AndroidId && !request.ShouldDeviceBeLinkedToUser)
            {
                Logger.Info(string.Format("User {0}[{1}] is linked to device {2}, but handshake was requested from device {3}",
                        interviewerInfo.UserName, interviewerInfo.PublicKey, interviewerInfo.DeviceId, request.AndroidId));

                throw CreateRestException(HttpStatusCode.NotAcceptable, InterviewerSyncStrings.WrongAndroidDeviceIdWasProvided);
            }

            var identifier = new ClientIdentifier
            {
                AndroidId = request.AndroidId,
                ClientInstanceKey = request.ClientId,
                AppVersion = request.Version.ToString(CultureInfo.InvariantCulture),
                ClientRegistrationKey = request.ClientRegistrationId,
                UserId = interviewerInfo.PublicKey
            };

            if (string.IsNullOrEmpty(interviewerInfo.DeviceId) || request.ShouldDeviceBeLinkedToUser)
            {
                this.syncManager.LinkUserToDevice(interviewerInfo.PublicKey, request.AndroidId, identifier.AppVersion, interviewerInfo.DeviceId);
            }

            return syncManager.InitSync(identifier);
        }

        [HttpGet]
        [ApiBasicAuth]
        public bool CheckExpectedDevice(string deviceId)
        {
            var interviewerInfo = userInfoViewFactory.Load(new UserWebViewInputModel(this.GlobalInfo.GetCurrentUser().Name, null));
            return string.IsNullOrEmpty(interviewerInfo.DeviceId) || interviewerInfo.DeviceId == deviceId;
        }

        [HttpPost]
        [ApiBasicAuth]
        public UserSyncPackageDto GetUserSyncPackage(SyncPackageRequest request)
        {
            return this.syncManager.ReceiveUserSyncPackage(request.ClientRegistrationId, request.PackageId, this.GlobalInfo.GetCurrentUser().Id);
        }

        [HttpPost]
        [ApiBasicAuth]
        public QuestionnaireSyncPackageDto GetQuestionnaireSyncPackage(SyncPackageRequest request)
        {
            return this.syncManager.ReceiveQuestionnaireSyncPackage(request.ClientRegistrationId, request.PackageId,
                this.GlobalInfo.GetCurrentUser().Id);
        }

        [HttpPost]
        [ApiBasicAuth]
        public InterviewSyncPackageDto GetInterviewSyncPackage(SyncPackageRequest request)
        {
            return this.syncManager.ReceiveInterviewSyncPackage(request.ClientRegistrationId, request.PackageId,
                this.GlobalInfo.GetCurrentUser().Id);
        }

        [HttpPost]
        [ApiBasicAuth]
        public SyncItemsMetaContainer GetUserPackageIds(SyncItemsMetaContainerRequest request)
        {
            try
            {
                return this.syncManager.GetUserPackageIdsWithOrder(this.GlobalInfo.GetCurrentUser().Id, request.ClientRegistrationId, request.LastSyncedPackageId);
            }
            catch (SyncPackageNotFoundException ex)
            {
                this.Logger.Error(ex.Message, ex);
                throw CreateRestException(HttpStatusCode.NotFound, InterviewerSyncStrings.ServerError);
            }
        }

        [HttpPost]
        [ApiBasicAuth]
        public SyncItemsMetaContainer GetQuestionnairePackageIds(SyncItemsMetaContainerRequest request)
        {
            try
            {
                return this.syncManager.GetQuestionnairePackageIdsWithOrder(this.GlobalInfo.GetCurrentUser().Id, request.ClientRegistrationId, request.LastSyncedPackageId);
            }
            catch (SyncPackageNotFoundException ex)
            {
                this.Logger.Error(ex.Message, ex);
                throw CreateRestException(HttpStatusCode.NotFound, InterviewerSyncStrings.ServerError);
            }
        }

        [HttpPost]
        [ApiBasicAuth]
        public SyncItemsMetaContainer GetInterviewPackageIds(SyncItemsMetaContainerRequest request)
        {
            try
            {
                return this.syncManager.GetInterviewPackageIdsWithOrder(this.GlobalInfo.GetCurrentUser().Id, request.ClientRegistrationId, request.LastSyncedPackageId);
            }
            catch (SyncPackageNotFoundException ex)
            {
                this.Logger.Error(ex.Message, ex);
                throw CreateRestException(HttpStatusCode.NotFound, InterviewerSyncStrings.ServerError);
            }
        }

        [HttpPost]
        [ApiBasicAuth]
        public HttpResponseMessage PostFile(PostFileRequest request)
        {
            plainFileRepository.StoreInterviewBinaryData(request.InterviewId, request.FileName, Convert.FromBase64String(request.Data));
            return Request.CreateResponse(HttpStatusCode.OK);
        }

        [HttpPost]
        [ApiBasicAuth]
        public HttpResponseMessage PostPackage(PostPackageRequest request)
        {
            syncManager.SendSyncItem(interviewId: request.InterviewId, package: request.SynchronizationPackage);

            return Request.CreateResponse(HttpStatusCode.OK);
        }

        [HttpGet]
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

            return Request.CreateErrorResponse(HttpStatusCode.NotFound, InterviewerSyncStrings.FileWasNotFound);
        }

        [HttpGet]
        public bool CheckNewVersion(int versionCode)
        {
            string targetToSearchCapi = fileSystemAccessor.CombinePath(HostingEnvironment.MapPath(pathToSearchVersions), CapiFileName);

            int? supervisorRevisionNumber = versionProvider.GetApplicationBuildNumber();

            return fileSystemAccessor.IsFileExists(targetToSearchCapi) && supervisorRevisionNumber.HasValue && (supervisorRevisionNumber.Value > versionCode);
        }

        [HttpPost]
        public void PostInfoPackage(TabletInformationPackage tabletInformationPackage)
        {
            this.tabletInformationService.SaveTabletInformation(
                content: Convert.FromBase64String(tabletInformationPackage.Content),
                androidId: tabletInformationPackage.AndroidId,
                registrationId: tabletInformationPackage.ClientRegistrationId.ToString());
        }
    }
}