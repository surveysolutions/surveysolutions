using System;
using System.Globalization;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Web.Hosting;
using System.Web.Http;
using WB.Core.GenericSubdomains.Utils;
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
        private readonly IJsonUtils jsonUtils;


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
            ITabletInformationService tabletInformationService,
            IJsonUtils jsonUtils)
            : base(commandService, globalInfo, logger)
        {

            this.plainFileRepository = plainFileRepository;
            this.versionProvider = versionProvider;
            this.syncManager = syncManager;
            this.userInfoViewFactory = userInfoViewFactory;
            this.fileSystemAccessor = fileSystemAccessor;
            this.tabletInformationService = tabletInformationService;
            this.syncVersionProvider = syncVersionProvider;
            this.jsonUtils = jsonUtils;
        }

        [HttpGet]
        [ApiBasicAuth]
        [Obsolete]
        public HttpResponseMessage GetHandshakePackage(string clientId, string androidId, Guid? clientRegistrationId, int version = 0)
        {
            int supervisorRevisionNumber = syncVersionProvider.GetProtocolVersion();

            Logger.Info(string.Format("Old version client. Client has protocol version {0} but current app protocol is {1} ", version, supervisorRevisionNumber));

            return Request.CreateErrorResponse(HttpStatusCode.NotAcceptable,
                InterviewerSyncStrings.InterviewerApplicationHasVersion_butSupervisorHas_PleaseUpdateInterviewerApplication);
        }

        private HttpResponseException CreateRestException(HttpStatusCode httpStatusCode, SyncStatusCode code, string message)
        {
            var restErrorDescription = new RestErrorDescription { Code = code, Message = message };
            return new HttpResponseException(new HttpResponseMessage(httpStatusCode)
                                             {
                                                 ReasonPhrase = jsonUtils.Serialize(restErrorDescription)
                                             });
        }

        [HttpPost]
        [ApiBasicAuth]
        public HandshakePackage GetHandshakePackage(HandshakePackageRequest request)
        {
            int supervisorRevisionNumber = syncVersionProvider.GetProtocolVersion();

            if (request.Version > supervisorRevisionNumber)
            {
                Logger.Error(string.Format("Version mismatch. Client from the future. Client has protocol version {0} but current app protocol is {1} ", request.Version, supervisorRevisionNumber));

                throw CreateRestException(HttpStatusCode.NotAcceptable, SyncStatusCode.General, InterviewerSyncStrings.InterviewerApplicationHasHigherVersion_thanSupervisor_Format);
            }

            if (request.Version < supervisorRevisionNumber)
            {
                Logger.Info(string.Format(" Client has protocol version {0} but current app protocol is {1} ", request.Version, supervisorRevisionNumber));

                throw CreateRestException(
                    HttpStatusCode.NotAcceptable,
                    SyncStatusCode.General,
                    InterviewerSyncStrings.InterviewerApplicationHasVersion_butSupervisorHas_PleaseUpdateInterviewerApplication);
            }

            if (string.IsNullOrEmpty(request.AndroidId))
            {
                Logger.Info(string.Format("Android device id was not provided"));

                 throw CreateRestException(
                    HttpStatusCode.NotAcceptable,
                    SyncStatusCode.General,
                    InterviewerSyncStrings.AndroidDeviceIdWasNotProvided
                );
            }

            var interviewerInfo = userInfoViewFactory.Load(new UserWebViewInputModel(this.GlobalInfo.GetCurrentUser().Name, null));

            if (!string.IsNullOrEmpty(interviewerInfo.DeviceId) && interviewerInfo.DeviceId != request.AndroidId && !request.ShouldDeviceBeLinkedToUser)
            {
                Logger.Info(string.Format("User {0}[{1}] is linked to device {2}, but handshake was requested from device {3}",
                    interviewerInfo.UserName, interviewerInfo.PublicKey, interviewerInfo.DeviceId, request.AndroidId));

                throw CreateRestException(
                    HttpStatusCode.NotAcceptable,
                    SyncStatusCode.DeviceIsNotLinkedToUser,
                    InterviewerSyncStrings.WrongAndroidDeviceIdWasProvided
                );
            }

            var identifier = new ClientIdentifier
            {
                AndroidId = request.AndroidId,
                ClientInstanceKey = request.ClientId,
                AppVersion = request.Version.ToString(CultureInfo.InvariantCulture),
                ClientRegistrationKey = request.ClientRegistrationId,
                SupervisorPublicKey = interviewerInfo.Supervisor.Id,
                UserId = interviewerInfo.PublicKey
            };
            try
            {
                if (string.IsNullOrEmpty(interviewerInfo.DeviceId) || request.ShouldDeviceBeLinkedToUser)
                {
                    this.syncManager.LinkUserToDevice(interviewerInfo.PublicKey, request.AndroidId, interviewerInfo.DeviceId);
                }
                return syncManager.ItitSync(identifier);
            }
            catch (Exception exc)
            {
                Logger.Fatal(
                    string.Format("Sync Handshake Error. ClientId:{0}, AndroidId : {1}, ClientRegistrationId:{2}, version: {3}",
                        request.ClientId, request.AndroidId, request.ClientRegistrationId, request.Version), exc);

                throw CreateRestException(HttpStatusCode.InternalServerError, SyncStatusCode.General, exc.Message);
            }
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
            try
            {
                var interviewerInfo = userInfoViewFactory.Load(new UserWebViewInputModel(this.GlobalInfo.GetCurrentUser().Name, null));
                return syncManager.ReceiveUserSyncPackage(request.ClientRegistrationId, request.PackageId, interviewerInfo.PublicKey);
            }
            catch (Exception ex)
            {
                Logger.Error(ex.Message, ex);
                throw CreateRestException(HttpStatusCode.ServiceUnavailable, SyncStatusCode.General, InterviewerSyncStrings.ServerError);
            }
        }

        [HttpPost]
        [ApiBasicAuth]
        public QuestionnaireSyncPackageDto GetQuestionnaireSyncPackage(SyncPackageRequest request)
        {
            try
            {
                var interviewerInfo = userInfoViewFactory.Load(new UserWebViewInputModel(this.GlobalInfo.GetCurrentUser().Name, null));
                return syncManager.ReceiveQuestionnaireSyncPackage(request.ClientRegistrationId, request.PackageId, interviewerInfo.PublicKey);
            }
            catch (Exception ex)
            {
                Logger.Error(ex.Message, ex);
                throw CreateRestException(HttpStatusCode.ServiceUnavailable, SyncStatusCode.General, InterviewerSyncStrings.ServerError);
            }
        }

        [HttpPost]
        [ApiBasicAuth]
        public InterviewSyncPackageDto GetInterviewSyncPackage(SyncPackageRequest request)
        {
            try
            {
                var interviewerInfo = userInfoViewFactory.Load(new UserWebViewInputModel(this.GlobalInfo.GetCurrentUser().Name, null));
                return syncManager.ReceiveInterviewSyncPackage(request.ClientRegistrationId, request.PackageId, interviewerInfo.PublicKey);
            }
            catch (Exception ex)
            {
                Logger.Error(ex.Message, ex);
                throw CreateRestException(HttpStatusCode.ServiceUnavailable, SyncStatusCode.General, InterviewerSyncStrings.ServerError);
            }
        }

        [HttpPost]
        [ApiBasicAuth]
        public SyncItemsMetaContainer GetUserArKeys(SyncItemsMetaContainerRequest request)
        {
            try
            {
                return this.syncManager.GetUserArIdsWithOrder(this.GlobalInfo.GetCurrentUser().Id, request.ClientRegistrationId, request.LastSyncedPackageId);
            }
            catch (SyncPackageNotFoundException ex)
            {
                this.Logger.Error(ex.Message, ex);
                throw this.CreateRestException(HttpStatusCode.ServiceUnavailable, SyncStatusCode.General, InterviewerSyncStrings.ServerError);
            }
        }

        [HttpPost]
        [ApiBasicAuth]
        public SyncItemsMetaContainer GetQuestionnaireArKeys(SyncItemsMetaContainerRequest request)
        {
            try
            {
                return this.syncManager.GetQuestionnaireArIdsWithOrder(this.GlobalInfo.GetCurrentUser().Id, request.ClientRegistrationId, request.LastSyncedPackageId);
            }
            catch (SyncPackageNotFoundException ex)
            {
                this.Logger.Error(ex.Message, ex);
                throw this.CreateRestException(HttpStatusCode.ServiceUnavailable, SyncStatusCode.General, InterviewerSyncStrings.ServerError);
            }
        }

        [HttpPost]
        [ApiBasicAuth]
        public SyncItemsMetaContainer GetInterviewArKeys(SyncItemsMetaContainerRequest request)
        {
            try
            {
                return this.syncManager.GetInterviewArIdsWithOrder(this.GlobalInfo.GetCurrentUser().Id, request.ClientRegistrationId, request.LastSyncedPackageId);
            }
            catch (SyncPackageNotFoundException ex)
            {
                this.Logger.Error(ex.Message, ex);
                throw this.CreateRestException(HttpStatusCode.ServiceUnavailable, SyncStatusCode.General, InterviewerSyncStrings.ServerError);
            }
        }

        [HttpGet]
        [ApiBasicAuth]
        public string GetPackageIdByTimeStamp(long timestamp)
        {
            return this.syncManager.GetPackageIdByTimestamp(this.GlobalInfo.GetCurrentUser().Id, new DateTime(timestamp));
        }

        [HttpPost]
        [ApiBasicAuth]
        public void PostFile(PostFileRequest request)
        {
            try
            {
                plainFileRepository.StoreInterviewBinaryData(request.InterviewId, request.FileName, Convert.FromBase64String(request.Data));
            }
            catch (Exception ex)
            {
                Logger.Error(ex.Message, ex);

                throw CreateRestException(HttpStatusCode.ServiceUnavailable, SyncStatusCode.General, InterviewerSyncStrings.ServerError);
            }
        }

        [HttpPost]
        [ApiBasicAuth]
        public void PostPackage(PostPackageRequest request)
        {
            try
            {
                syncManager.SendSyncItem(request.SynchronizationPackage);
            }
            catch (Exception ex)
            {
                Logger.Error(ex.Message, ex);
                throw CreateRestException(HttpStatusCode.ServiceUnavailable, SyncStatusCode.General, InterviewerSyncStrings.ServerError);
            }
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