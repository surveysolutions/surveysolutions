using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Web.Hosting;
using System.Web.Http;
using System.Web.Security;
using Main.Core.View;
using Main.Core.View.User;
using Ncqrs.Commanding.ServiceModel;
using Newtonsoft.Json;
using WB.Core.GenericSubdomains.Logging;
using WB.Core.Infrastructure.FileSystem;
using WB.Core.SharedKernel.Structures.Synchronization;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.SurveyManagement.Services;
using WB.Core.SharedKernels.SurveyManagement.Web.Code;
using WB.Core.SharedKernels.SurveyManagement.Web.Controllers;
using WB.Core.SharedKernels.SurveyManagement.Web.Utils.Membership;
using WB.Core.Synchronization;

namespace WB.Core.SharedKernels.SurveyManagement.Web.Api
{
    public class InterviewerSyncController : BaseApiController
    {
        private readonly ILogger logger;
        private readonly ISyncManager syncManager;
        private readonly IViewFactory<UserViewInputModel, UserView> viewFactory;
        private readonly Func<string, string, bool> checkIfUserIsInRole;
        private readonly ISupportedVersionProvider versionProvider;
        private readonly IPlainInterviewFileStorage plainFileRepository;
        private readonly IGlobalInfoProvider globalInfo;
        private readonly IFileSystemAccessor fileSystemAccessor;


        private string CapiFileName = "wbcapi.apk";
        private string pathToSearchVersions = ("~/App_Data/Capi");

        public InterviewerSyncController(ICommandService commandService, IGlobalInfoProvider globalInfo,
            ISyncManager syncManager,
            ILogger logger, IViewFactory<UserViewInputModel, UserView> viewFactory,
            ISupportedVersionProvider versionProvider, IPlainInterviewFileStorage plainFileRepository,
            IFileSystemAccessor fileSystemAccessor)
            : this(commandService, globalInfo, syncManager, logger, viewFactory, versionProvider,
                Roles.IsUserInRole, plainFileRepository, fileSystemAccessor)
        {
        }

        public InterviewerSyncController(ICommandService commandService, IGlobalInfoProvider globalInfo,
            ISyncManager syncManager,ILogger logger,
            IViewFactory<UserViewInputModel, UserView> viewFactory, ISupportedVersionProvider versionProvider,
            Func<string, string, bool> checkIfUserIsInRole,IPlainInterviewFileStorage plainFileRepository,
            IFileSystemAccessor fileSystemAccessor)
            : base(commandService, globalInfo, logger)
        {
            this.checkIfUserIsInRole = checkIfUserIsInRole;
            this.plainFileRepository = plainFileRepository;
            this.versionProvider = versionProvider;
            this.syncManager = syncManager;
            this.logger = logger;
            this.viewFactory = viewFactory;
            this.globalInfo = globalInfo;
            this.fileSystemAccessor = fileSystemAccessor;
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
                return Request.CreateErrorResponse(HttpStatusCode.Forbidden, "Invalid user");

            Guid key;
            int? supervisorRevisionNumber = versionProvider.GetApplicationBuildNumber();

            if (supervisorRevisionNumber.HasValue && version > supervisorRevisionNumber.Value)
            {
                return Request.CreateErrorResponse(HttpStatusCode.NotAcceptable,
                    "Your application is incompatible with the Supervisor. Please, remove your copy and download the correct version");
            }

            if (supervisorRevisionNumber.HasValue && version < supervisorRevisionNumber.Value)
            {
                return Request.CreateErrorResponse(HttpStatusCode.NotAcceptable,
                    "You must update your CAPI application before synchronizing with the Supervisor");
            }

            if (!Guid.TryParse(clientId, out key))
            {
                return Request.CreateErrorResponse(HttpStatusCode.NotAcceptable,
                    "Client Identifier was not provided");
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
                        "Error occurred during synchronization initialization.");

                return Request.CreateResponse(HttpStatusCode.OK, package);
            }
            catch (Exception exc)
            {
                logger.Fatal("Sync Handshake Error", exc);

                return Request.CreateErrorResponse(HttpStatusCode.ServiceUnavailable,
                    "Error occurred during synchronization initialization.");
            }
        }

        [HttpGet]
        [ApiBasicAuth]
        public HttpResponseMessage GetSyncPackage(Guid aRKey, long aRTimestamp, string clientRegistrationId)
        {
            Guid clientRegistrationKey;
            if (!Guid.TryParse(clientRegistrationId, out clientRegistrationKey))
            {
                return Request.CreateErrorResponse(HttpStatusCode.NotAcceptable, "Invalid device identifier");
            }
            
            try
            {
                SyncPackage package = syncManager.ReceiveSyncPackage(clientRegistrationKey, aRKey, new DateTime(aRTimestamp));

                if (package == null)
                    return Request.CreateErrorResponse(HttpStatusCode.ServiceUnavailable,
                        "General error occurred. Try later");

                return Request.CreateResponse(HttpStatusCode.OK, package);
            }
            catch (Exception ex)
            {
                logger.Fatal("Error on sync", ex);
                logger.Fatal(ex.StackTrace);

                return Request.CreateErrorResponse(HttpStatusCode.ServiceUnavailable, "General error occurred. Try later");
            }
        }

        [HttpGet]
        [ApiBasicAuth]
        public HttpResponseMessage GetARKeys(string clientRegistrationId, string sequence)
        {
            UserView user = GetUser(globalInfo.GetCurrentUser().Name);
            if (user == null)
                return Request.CreateErrorResponse(HttpStatusCode.Forbidden, "Invalid user");

            Guid clientRegistrationKey;
            if (!Guid.TryParse(clientRegistrationId, out clientRegistrationKey) || clientRegistrationKey == Guid.Empty)
            {
                return Request.CreateErrorResponse(HttpStatusCode.NotAcceptable, "Invalid device identifier");
            }

            long clientSequence;
            if (!string.IsNullOrWhiteSpace(sequence))
            {
                if (!long.TryParse(sequence, out clientSequence))
                {
                    return Request.CreateErrorResponse(HttpStatusCode.NotAcceptable, "Invalid sequence identifier");
                }
            }
            else
            {
                clientSequence = 0;
            }

            try
            {
                IEnumerable<SynchronizationChunkMeta> package =
                    syncManager.GetAllARIdsWithOrder(user.PublicKey, clientRegistrationKey, new DateTime(clientSequence));
                var result = new SyncItemsMetaContainer {ChunksMeta = package.ToList()};

                return Request.CreateResponse(HttpStatusCode.OK, result);
            }
            catch (Exception ex)
            {
                logger.Fatal("Error on sync", ex);
                logger.Fatal(ex.StackTrace);

                return Request.CreateErrorResponse(HttpStatusCode.ServiceUnavailable,
                    "General error occurred. Try later");
            }
        }

        [HttpPost]
        [ApiBasicAuth]
        public async Task<HttpResponseMessage> PostFile([FromUri]Guid interviewId)
        {
            if (Request.Content == null || !Request.Content.IsMimeMultipartContent())
            {
                return Request.CreateErrorResponse(HttpStatusCode.NotAcceptable, "Incorrect media type");
            }

            try
            {
                var provider = new MultipartMemoryStreamProvider();
                await Request.Content.ReadAsMultipartAsync(provider);

                if (provider.Contents.Count != 1)
                    return Request.CreateErrorResponse(HttpStatusCode.NotAcceptable, "Incorrect files count");

                HttpContent file = provider.Contents[0];
                string filename = file.Headers.ContentDisposition.FileName.Trim('\"');
                byte[] buffer = await file.ReadAsByteArrayAsync();

                plainFileRepository.StoreInterviewBinaryData(interviewId, filename, buffer);

                return Request.CreateResponse(HttpStatusCode.OK, true);
            }
            catch (Exception ex)
            {
                logger.Fatal("Error on Sync.", ex);
                logger.Fatal("Exception message: " + ex.Message);
                logger.Fatal("Stack: " + ex.StackTrace);

                return Request.CreateErrorResponse(HttpStatusCode.ServiceUnavailable, "General error occurred. Try later");
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
                    return Request.CreateErrorResponse(HttpStatusCode.ServiceUnavailable,
                        "General error occurred. Try later");
                }

                return Request.CreateResponse(HttpStatusCode.OK, syncManager.SendSyncItem(syncItem));
            }
            catch (Exception ex)
            {
                logger.Fatal("Error on Sync.", ex);
                logger.Fatal("Exception message: " + ex.Message);
                logger.Fatal("Stack: " + ex.StackTrace);

                return Request.CreateErrorResponse(HttpStatusCode.ServiceUnavailable,
                    "General error occurred. Try later");
            }
        }

        [HttpGet]
        public HttpResponseMessage GetLatestVersion()
        {
            int maxVersion = GetLastVersionNumber();

            if (maxVersion <= 0)
                return Request.CreateErrorResponse(HttpStatusCode.NotFound, "File was not found");


            string targetToSearchVersions = HostingEnvironment.MapPath(pathToSearchVersions);

            string path = fileSystemAccessor.CombinePath(targetToSearchVersions, maxVersion.ToString(CultureInfo.InvariantCulture));
            string pathToFile = fileSystemAccessor.CombinePath(path, CapiFileName);

            if (fileSystemAccessor.IsFileExists(pathToFile))
            {
                var response = new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StreamContent(fileSystemAccessor.ReadFile(pathToFile))
                };

                response.Content.Headers.ContentType =
                    new MediaTypeHeaderValue("application/vnd.android.package-archive");

                return response;
            }
            
            return Request.CreateErrorResponse(HttpStatusCode.NotFound, "File was not found");
        }

        private int GetLastVersionNumber()
        {
            int maxVersion = 0;

            string targetToSearchVersions = HostingEnvironment.MapPath(pathToSearchVersions);
            
            if (fileSystemAccessor.IsDirectoryExists(targetToSearchVersions))
            {
                var containingDirectories = fileSystemAccessor.GetDirectoriesInDirectory(targetToSearchVersions);
                
                foreach (var directoryInfo in containingDirectories)
                {
                    int value;
                    if (int.TryParse(fileSystemAccessor.GetFileName(directoryInfo), out value))
                        if (maxVersion < value)
                            maxVersion = value;
                }
            }

            return maxVersion;
        }

        [HttpGet]
        public HttpResponseMessage CheckNewVersion(int versionCode)
        {
            int maxVersion = GetLastVersionNumber();
            return Request.CreateResponse(HttpStatusCode.OK, (maxVersion != 0 && maxVersion > versionCode));
        }
    }
}