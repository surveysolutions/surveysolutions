using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Web.Hosting;
using System.Web.Http;
using WB.Core.GenericSubdomains.Utils.Services;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.Infrastructure.FileSystem;
using WB.Core.Infrastructure.ReadSide;
using WB.Core.SharedKernel.Structures.Synchronization;
using WB.Core.SharedKernel.Structures.Synchronization.SurveyManagement;
using WB.Core.SharedKernel.Structures.TabletInformation;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.SurveyManagement.Services;
using WB.Core.SharedKernels.SurveyManagement.Web.Code;
using WB.Core.SharedKernels.SurveyManagement.Web.Controllers;
using WB.Core.SharedKernels.SurveyManagement.Web.Models.User;
using WB.Core.SharedKernels.SurveyManagement.Web.Properties;
using WB.Core.SharedKernels.SurveyManagement.Web.Resources;
using WB.Core.SharedKernels.SurveyManagement.Web.Utils.Membership;
using WB.Core.Synchronization;
using WB.Core.Synchronization.SyncStorage;

namespace WB.Core.SharedKernels.SurveyManagement.Web.Api
{
    public class InterviewerSyncController : BaseApiController
    {
        private readonly ISyncManager syncManager;
        private readonly IViewFactory<UserViewInputModel, UserView> userInfoViewFactory;
        private readonly ISupportedVersionProvider versionProvider;
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
            IViewFactory<UserViewInputModel, UserView> userInfoViewFactory, 
            ISupportedVersionProvider versionProvider,
            IPlainInterviewFileStorage plainFileRepository,
            IFileSystemAccessor fileSystemAccessor,
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
            this.jsonUtils = jsonUtils;
        }

        [HttpPost]
        [ApiBasicAuth]
        public HandshakePackage GetHandshakePackage(HandshakePackageRequest request)
        {
            int? supervisorRevisionNumber = versionProvider.GetApplicationBuildNumber();

            if (supervisorRevisionNumber.HasValue && request.Version > supervisorRevisionNumber.Value)
            {
                throw new HttpResponseException(new HttpResponseMessage(HttpStatusCode.NotAcceptable)
                {
                    ReasonPhrase = string.Format(InterviewerSyncStrings.InterviewerApplicationHasHigherVersion_thanSupervisor_Format, request.Version, supervisorRevisionNumber.Value)
                });
            }

            if (supervisorRevisionNumber.HasValue && request.Version < supervisorRevisionNumber.Value)
            {
                throw new HttpResponseException(new HttpResponseMessage(HttpStatusCode.NotAcceptable)
                {
                    ReasonPhrase = string.Format(InterviewerSyncStrings.InterviewerApplicationHasVersion_butSupervisorHas_PleaseUpdateInterviewerApplication, request.Version, supervisorRevisionNumber.Value)
                });
            }

            var interviewerInfo = userInfoViewFactory.Load(new UserViewInputModel(this.GlobalInfo.GetCurrentUser().Name, null));

            var identifier = new ClientIdentifier
            {
                ClientDeviceKey = request.AndroidId,
                ClientInstanceKey = request.ClientId,
                ClientVersionIdentifier = "unknown",
                ClientRegistrationKey = request.ClientRegistrationId,
                SupervisorPublicKey = interviewerInfo.Supervisor.Id
            };
            try
            {
                return syncManager.ItitSync(identifier);
            }
            catch (Exception exc)
            {
                Logger.Fatal(
                    string.Format("Sync Handshake Error. ClientId:{0}, AndroidId : {1}, ClientRegistrationId:{2}, version: {3}",
                        request.ClientId, request.AndroidId, request.ClientRegistrationId, request.Version), exc);

                throw new HttpResponseException(new HttpResponseMessage(HttpStatusCode.InternalServerError)
                {
                    ReasonPhrase = exc.Message
                });
            }
        }

        [HttpPost]
        [ApiBasicAuth]
        public SyncPackage GetSyncPackage(SyncPackageRequest request)
        {
            try
            {
                return syncManager.ReceiveSyncPackage(request.ClientRegistrationId, request.PackageId);
            }
            catch (Exception ex)
            {
                Logger.Error(ex.Message, ex);
                throw new HttpResponseException(new HttpResponseMessage(HttpStatusCode.ServiceUnavailable)
                {
                    ReasonPhrase = InterviewerSyncStrings.ServerError
                });
            }
        }

        [HttpPost]
        [ApiBasicAuth]
        public SyncItemsMetaContainer GetARKeys(SyncItemsMetaContainerRequest request)
        {
            try
            {
                IEnumerable<SynchronizationChunkMeta> package = syncManager.GetAllARIdsWithOrder(this.GlobalInfo.GetCurrentUser().Id,
                    request.ClientRegistrationId, request.LastSyncedPackageId);

                return new SyncItemsMetaContainer {ChunksMeta = package};
            }
            catch (SyncPackageNotFoundException ex)
            {
                Logger.Error(ex.Message, ex);
                throw new HttpResponseException(new HttpResponseMessage(HttpStatusCode.ServiceUnavailable)
                {
                    ReasonPhrase = InterviewerSyncStrings.ServerError
                });
            }
        }

        [HttpGet]
        [ApiBasicAuth]
        public string GetPacakgeIdByTimeStamp(long timestamp)
        {
            return this.syncManager.GetPackageIdByTimestamp(new DateTime(timestamp));
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
                throw new HttpResponseException(new HttpResponseMessage(HttpStatusCode.ServiceUnavailable)
                {
                    ReasonPhrase = InterviewerSyncStrings.ServerError
                });
            }
        }

        [HttpPost]
        [ApiBasicAuth]
        public void PostPackage(PostPackageRequest request)
        {
            try
            {
                var syncItem = this.jsonUtils.Deserialize<SyncItem>(request.SynchronizationPackage);

                syncManager.SendSyncItem(syncItem);
            }
            catch (Exception ex)
            {
                Logger.Error(ex.Message, ex);
                throw new HttpResponseException(new HttpResponseMessage(HttpStatusCode.ServiceUnavailable)
                {
                    ReasonPhrase = InterviewerSyncStrings.ServerError
                });
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