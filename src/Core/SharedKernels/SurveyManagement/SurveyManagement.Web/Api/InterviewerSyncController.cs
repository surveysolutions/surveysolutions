using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Web.Hosting;
using System.Web.Http;
using WB.Core.GenericSubdomains.Logging;
using WB.Core.GenericSubdomains.Utils;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.Infrastructure.FileSystem;
using WB.Core.Infrastructure.ReadSide;
using WB.Core.SharedKernel.Structures.Synchronization;
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
        private readonly ILogger logger;
        private readonly ISyncManager syncManager;
        private readonly IViewFactory<UserViewInputModel, UserView> viewFactory;
        private readonly ISupportedVersionProvider versionProvider;
        private readonly IPlainInterviewFileStorage plainFileRepository;
        private readonly IGlobalInfoProvider globalInfo;
        private readonly IFileSystemAccessor fileSystemAccessor;
        private readonly ITabletInformationService tabletInformationService;

        private string ResponseInterviewerFileName = "interviewer.apk";
        private string CapiFileName = "wbcapi.apk";
        private string pathToSearchVersions = ("~/Client/");


        public InterviewerSyncController(ICommandService commandService, 
            IGlobalInfoProvider globalInfo,
            ISyncManager syncManager,
            ILogger logger,
            IViewFactory<UserViewInputModel, UserView> viewFactory, 
            ISupportedVersionProvider versionProvider,
            IPlainInterviewFileStorage plainFileRepository,
            IFileSystemAccessor fileSystemAccessor,
            ITabletInformationService tabletInformationService)
            : base(commandService, globalInfo, logger)
        {
            
            this.plainFileRepository = plainFileRepository;
            this.versionProvider = versionProvider;
            this.syncManager = syncManager;
            this.logger = logger;
            this.viewFactory = viewFactory;
            this.globalInfo = globalInfo;
            this.fileSystemAccessor = fileSystemAccessor;
            this.tabletInformationService = tabletInformationService;
        }

        protected UserView GetUser(string login)
        {
            UserView user = viewFactory.Load(new UserViewInputModel(login, null));
            if (user.isLockedBySupervisor || user.IsLockedByHQ)
                return null;

            return user;
        }

        [HttpGet]
        [ApiBasicAuth]
        public HttpResponseMessage GetHandshakePackage(string clientId, string androidId, Guid? clientRegistrationId, int version = 0)
        {
            UserView user = GetUser(globalInfo.GetCurrentUser().Name);
            if (user == null)
                return Request.CreateErrorResponse(HttpStatusCode.Unauthorized, FieldsAndValidations.InvalidUser);

            Guid key;
            int? supervisorRevisionNumber = versionProvider.GetApplicationBuildNumber();

            if (supervisorRevisionNumber.HasValue && version > supervisorRevisionNumber.Value)
            {
                return Request.CreateErrorResponse(HttpStatusCode.NotAcceptable, string.Format(InterviewerSyncControllerMessages.InterviewerApplicationHasHigherVersion_thanSupervisor_Format, version, supervisorRevisionNumber));
            }

            if (supervisorRevisionNumber.HasValue && version < supervisorRevisionNumber.Value)
            {
                return Request.CreateErrorResponse(HttpStatusCode.NotAcceptable, string.Format(InterviewerSyncControllerMessages.InterviewerApplicationHasVersion_butSupervisorHas_PleaseUpdateInterviewerApplication, version, supervisorRevisionNumber));
            }

            if (!Guid.TryParse(clientId, out key))
            {
                return Request.CreateErrorResponse(HttpStatusCode.NotAcceptable, InterviewerSyncControllerMessages.NoClientIdentifier);
            }

            var identifier = new ClientIdentifier();
            identifier.ClientDeviceKey = androidId;
            identifier.ClientInstanceKey = key;
            identifier.ClientVersionIdentifier = "unknown";
            identifier.ClientRegistrationKey = clientRegistrationId;
            identifier.SupervisorPublicKey = user.Supervisor.Id;

            try
            {
                HandshakePackage package = syncManager.ItitSync(identifier);
                if (package == null)
                    return Request.CreateErrorResponse(HttpStatusCode.ServiceUnavailable,
                        InterviewerSyncControllerMessages.ServerError);

                return Request.CreateResponse(HttpStatusCode.OK, package);
            }
            catch (Exception exc)
            {
                logger.Fatal(
                    string.Format("Sync Handshake Error. ClientId:{0}, AndroidId : {1}, ClientRegistrationId:{2}, version: {3}", clientId, androidId, clientRegistrationId, version), exc);

                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, exc.Message);
            }
        }

        [HttpGet]
        [ApiBasicAuth]
        public HttpResponseMessage GetSyncPackage(string packageId, string clientRegistrationId)
        {
            Guid clientRegistrationKey;
            if (!Guid.TryParse(clientRegistrationId, out clientRegistrationKey))
            {
                return Request.CreateErrorResponse(HttpStatusCode.NotAcceptable, InterviewerSyncControllerMessages.InvalidDeviceIdentifier);
            }
            
            try
            {
                SyncPackage package = syncManager.ReceiveSyncPackage(clientRegistrationKey, packageId);

                if (package == null)
                    return Request.CreateErrorResponse(HttpStatusCode.ServiceUnavailable,
                        string.Format(InterviewerSyncControllerMessages.Sync_package_with_id__0__was_not_found_on_serverFormat, packageId));

                return Request.CreateResponse(HttpStatusCode.OK, package);
            }
            catch (Exception ex)
            {
                logger.Error(ex.Message,ex);
                return Request.CreateErrorResponse(HttpStatusCode.ServiceUnavailable, InterviewerSyncControllerMessages.ServerError);
            }
        }

        [HttpGet]
        [ApiBasicAuth]
        public HttpResponseMessage GetARKeys(string clientRegistrationId, string lastSyncedPackageId)
        {
            UserView user = GetUser(globalInfo.GetCurrentUser().Name);
            if (user == null)
                return Request.CreateErrorResponse(HttpStatusCode.Unauthorized, InterviewerSyncControllerMessages.InvalidUser);

            Guid clientRegistrationKey;
            if (!Guid.TryParse(clientRegistrationId, out clientRegistrationKey) || clientRegistrationKey == Guid.Empty)
            {
                return Request.CreateErrorResponse(HttpStatusCode.NotAcceptable, InterviewerSyncControllerMessages.InvalidDeviceIdentifier);
            }

            try
            {
                IEnumerable<SynchronizationChunkMeta> package = syncManager.GetAllARIdsWithOrder(user.PublicKey, 
                    clientRegistrationKey, 
                    lastSyncedPackageId.NullIfEmptyOrWhiteSpace());

                var result = new SyncItemsMetaContainer
                {
                    ChunksMeta = package.ToList()
                };

                return Request.CreateResponse(HttpStatusCode.OK, result);
            }
            catch (SyncPackageNotFoundException ex)
            {
                return Request.CreateErrorResponse(HttpStatusCode.Gone, ex);
            }
            catch (Exception ex)
            {
                logger.Error(ex.Message, ex);

                return Request.CreateErrorResponse(HttpStatusCode.ServiceUnavailable,
                    InterviewerSyncControllerMessages.ServerError);
            }
        }

        [HttpGet]
        [ApiBasicAuth]
        public HttpResponseMessage GetPacakgeIdByTimeStamp(long timestamp)
        {
            var result = this.syncManager.GetPackageIdByTimestamp(new DateTime(timestamp));
            return Request.CreateResponse(HttpStatusCode.OK, result);
        }

        [HttpPost]
        [ApiBasicAuth]
        public async Task<HttpResponseMessage> PostFile([FromUri]Guid interviewId)
        {
            if (Request.Content == null || !Request.Content.IsMimeMultipartContent())
            {
                return Request.CreateErrorResponse(HttpStatusCode.NotAcceptable, InterviewerSyncControllerMessages.IncorrectMediaType);
            }

            try
            {
                var provider = new MultipartMemoryStreamProvider();
                await Request.Content.ReadAsMultipartAsync(provider);

                if (provider.Contents.Count != 1)
                    return Request.CreateErrorResponse(HttpStatusCode.NotAcceptable, InterviewerSyncControllerMessages.IncorrectFilesCount);

                HttpContent file = provider.Contents[0];
                string filename = file.Headers.ContentDisposition.FileName.Trim('\"');
                byte[] buffer = await file.ReadAsByteArrayAsync();

                plainFileRepository.StoreInterviewBinaryData(interviewId, filename, buffer);

                return Request.CreateResponse(HttpStatusCode.OK, true);
            }
            catch (Exception ex)
            {
                logger.Error(ex.Message, ex);

                return Request.CreateErrorResponse(HttpStatusCode.ServiceUnavailable, InterviewerSyncControllerMessages.ServerError);
            }
        }

        [HttpPost]
        [ApiBasicAuth]
        public async Task<HttpResponseMessage> PostPackage()
        {
            try
            {
                var syncItem = JsonConvert.DeserializeObject<SyncItem>(await Request.Content.ReadAsStringAsync(),
                    new JsonSerializerSettings
                    {
                        TypeNameHandling = TypeNameHandling.Objects
                    });


                if (syncItem == null)
                {
                    return Request.CreateErrorResponse(HttpStatusCode.ServiceUnavailable, InterviewerSyncControllerMessages.ServerError);
                }

                return Request.CreateResponse(HttpStatusCode.OK, syncManager.SendSyncItem(syncItem));
            }
            catch (Exception ex)
            {
                logger.Error(ex.Message, ex);

                return Request.CreateErrorResponse(HttpStatusCode.ServiceUnavailable, InterviewerSyncControllerMessages.ServerError);
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
            
            return Request.CreateErrorResponse(HttpStatusCode.NotFound, InterviewerSyncControllerMessages.FileWasNotFound);
        }
        
        [HttpGet]
        public HttpResponseMessage CheckNewVersion(int versionCode)
        {
            string targetToSearchCapi = fileSystemAccessor.CombinePath(HostingEnvironment.MapPath(pathToSearchVersions), CapiFileName);

            int? supervisorRevisionNumber = versionProvider.GetApplicationBuildNumber();

            bool newVersionExists = fileSystemAccessor.IsFileExists(targetToSearchCapi) &&
                                    supervisorRevisionNumber.HasValue &&
                                    (supervisorRevisionNumber.Value > versionCode);

            return Request.CreateResponse(HttpStatusCode.OK, newVersionExists);
        }
    }
}