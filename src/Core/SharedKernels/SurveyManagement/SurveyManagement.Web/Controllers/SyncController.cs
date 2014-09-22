using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Web.Mvc;
using System.Web.Security;
using Main.Core.Entities.SubEntities;
using Main.Core.View;
using Main.Core.View.User;
using Newtonsoft.Json;
using WB.Core.GenericSubdomains.Logging;
using WB.Core.SharedKernel.Structures.Synchronization;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.SurveyManagement.Services;
using WB.Core.Synchronization;
using WB.UI.Shared.Web.Exceptions;
using WB.UI.Shared.Web.Filters;

namespace WB.Core.SharedKernels.SurveyManagement.Web.Controllers
{
    public class SyncController : AsyncController
    {
        private readonly ILogger logger;
        private readonly ISyncManager syncManager;
        private readonly IViewFactory<UserViewInputModel, UserView> viewFactory;
        private readonly Func<string, string, bool> validateUserCredentials;
        private readonly Func<string, string, bool> checkIfUserIsInRole;
        private readonly ISupportedVersionProvider versionProvider;
        private readonly IPlainInterviewFileStorage plainFileRepository;

        private string CapiFileName = "wbcapi.apk";

        private string pathToSearchVersions = ("~/App_Data/Capi");

        public SyncController(ISyncManager syncManager,
            ILogger logger,
            IViewFactory<UserViewInputModel, UserView> viewFactory,
            ISupportedVersionProvider versionProvider, IPlainInterviewFileStorage plainFileRepository)
            : this(syncManager, logger, viewFactory, versionProvider, Membership.ValidateUser, Roles.IsUserInRole, plainFileRepository) { }

        public SyncController(ISyncManager syncManager, ILogger logger, IViewFactory<UserViewInputModel, UserView> viewFactory,
            ISupportedVersionProvider versionProvider, Func<string, string, bool> validateUserCredentials,
            Func<string, string, bool> checkIfUserIsInRole, IPlainInterviewFileStorage plainFileRepository)
        {
            this.validateUserCredentials = validateUserCredentials;
            this.checkIfUserIsInRole = checkIfUserIsInRole;
            this.plainFileRepository = plainFileRepository;
            this.versionProvider = versionProvider;
            this.syncManager = syncManager;
            this.logger = logger;
            this.viewFactory = viewFactory;
        }

        protected UserView GetUser(string login, string password)
        {
            if (this.validateUserCredentials(login, password))
            {
                if (this.checkIfUserIsInRole(login, UserRoles.Operator.ToString()))
                {
                    return this.viewFactory.Load(new UserViewInputModel(login, null));
                }
            }
            return null;
        }

        //In case of error of type missing or casting error we send correct response.
        [AcceptVerbs(HttpVerbs.Post)]
        [HandleUIException]
        public ActionResult Handshake(string clientId, string androidId, Guid? clientRegistrationId, int version = 0)
        {
            UserView user = this.GetUserByNameAndPassword();
            if (user == null)
                throw new HttpStatusException(HttpStatusCode.Forbidden);

            var package = new HandshakePackage();

            Guid key;
            int? supervisorRevisionNumber = this.versionProvider.GetApplicationBuildNumber();

            if (supervisorRevisionNumber.HasValue && version > supervisorRevisionNumber.Value)
            {
                package.IsErrorOccured = true;
                package.ErrorMessage = "Your application is incometible with the Supervisor. Please, remove your copy and download the correct version";
            }
            else if (supervisorRevisionNumber.HasValue && version < supervisorRevisionNumber.Value)
            {
                package.IsErrorOccured = true;
                package.ErrorMessage = "You must update your CAPI application before synchronizing with the Supervisor";
            }
            else if (!Guid.TryParse(clientId, out key))
            {
                package.IsErrorOccured = true;
                package.ErrorMessage = "Client Identifier was not provided.";
            }
            else
            {
                var identifier = new ClientIdentifier();
                identifier.ClientDeviceKey = androidId;
                identifier.ClientInstanceKey = key;
                identifier.ClientVersionIdentifier = "unknown";
                identifier.ClientRegistrationKey = clientRegistrationId;
                identifier.SupervisorPublicKey = user.Supervisor.Id;
                try
                {
                    package = this.syncManager.ItitSync(identifier);
                }
                catch (Exception exc)
                {
                    this.logger.Fatal("Sync Handshake Error", exc);
                    package.IsErrorOccured = true;
                    package.ErrorMessage = "Error occurred on sync. Try later.";
                }
            }
            return this.Json(package, JsonRequestBehavior.AllowGet);
        }

        [AcceptVerbs(HttpVerbs.Post)]
        [HandleUIException]
        public ActionResult InitPulling(string clientRegistrationId)
        {
            UserView user = this.GetUserByNameAndPassword();
            if (user == null)
                throw new HttpStatusException(HttpStatusCode.Forbidden);

            var package = new PullInitPackage();

            Guid clientRegistrationKey;
            if (!Guid.TryParse(clientRegistrationId, out clientRegistrationKey))
            {
                package.IsErrorOccured = true;
                package.ErrorMessage = "Invalid client identifier";
            }
            else
            {
                package.ItemsInQueue = 0;
            }

            return this.Json(package, JsonRequestBehavior.AllowGet);
        }

        //In case of error of type missing or casting error we send correct response.
        [AcceptVerbs(HttpVerbs.Post)]
        [HandleUIException]
        public ActionResult GetSyncPackage(string aRKey, string aRTimestamp, string clientRegistrationId)
        {
            UserView user = this.GetUserByNameAndPassword();
            if (user == null)
                throw new HttpStatusException(HttpStatusCode.Forbidden);

            var package = new SyncPackage();

            Guid key;
            if (!Guid.TryParse(aRKey, out key))
            {
                package.IsErrorOccured = true;
                package.ErrorMessage = "Invalid object identifier";
                return this.Json(package, JsonRequestBehavior.AllowGet);
            }

            Guid clientRegistrationKey;
            if (!Guid.TryParse(clientRegistrationId, out clientRegistrationKey))
            {
                package.IsErrorOccured = true;
                package.ErrorMessage = "Invalid device identifier";
                return this.Json(package, JsonRequestBehavior.AllowGet);
            }

            long timestamp;
            if (!long.TryParse(aRTimestamp, out timestamp))
            {
                package.IsErrorOccured = true;
                package.ErrorMessage = "Invalid sequence identifier";
                return this.Json(package, JsonRequestBehavior.AllowGet);
            }

            try
            {
                package = this.syncManager.ReceiveSyncPackage(clientRegistrationKey, key, new DateTime(timestamp));
            }
            catch (Exception ex)
            {
                this.logger.Fatal("Error on sync", ex);
                this.logger.Fatal(ex.StackTrace);

                package.IsErrorOccured = true;
                package.ErrorMessage = "Error occurred. Try later.";
            }

            return this.Json(package, JsonRequestBehavior.AllowGet);
        }

        //In case of error of type missing or casting error we send correct response.
        [AcceptVerbs(HttpVerbs.Post)]
        [HandleUIException]
        public JsonResult GetARKeys(string clientRegistrationId, string sequence)
        {
            UserView user = this.GetUserByNameAndPassword();
            if (user == null)
                throw new HttpStatusException(HttpStatusCode.Forbidden);

            Guid clientRegistrationKey;
            if (!Guid.TryParse(clientRegistrationId, out clientRegistrationKey))
            {
                var result = new SyncItemsMetaContainer();
                result.IsErrorOccured = true;
                return this.Json(result, JsonRequestBehavior.AllowGet);
            }

            if (clientRegistrationKey == Guid.Empty)
            {
                var result = new SyncItemsMetaContainer();
                result.IsErrorOccured = true;
                return this.Json(result, JsonRequestBehavior.AllowGet);
            }

            if (string.IsNullOrWhiteSpace(sequence))
                sequence = "0";

            long clientSequence;
            if (!long.TryParse(sequence, out clientSequence))
            {
                var result = new SyncItemsMetaContainer();
                result.IsErrorOccured = true;
                return this.Json(result, JsonRequestBehavior.AllowGet);
            }

            return this.Json(this.GetListOfAR(user.PublicKey, clientRegistrationKey, clientSequence), JsonRequestBehavior.AllowGet);
        }

        private SyncItemsMetaContainer GetListOfAR(Guid userId, Guid clientRegistrationKey, long timestamp)
        {
            var result = new SyncItemsMetaContainer();

            try
            {
                IEnumerable<SynchronizationChunkMeta> package = this.syncManager.GetAllARIdsWithOrder(userId, clientRegistrationKey,
                                                                                                      new DateTime(timestamp));
                result.ChunksMeta = package.ToList();
            }
            catch (Exception ex)
            {
                this.logger.Fatal("Error on retrieving the list of AR on sync. ", ex);
                this.logger.Fatal(ex.Message);
                this.logger.Fatal(ex.StackTrace);

                result.ErrorMessage = "Server error occurred.";
                result.IsErrorOccured = true;
            }

            return result;
        }
         //In case of error of type missing or casting error we send correct response.
        [AcceptVerbs(HttpVerbs.Post)]
        [HandleUIException]
        public ActionResult PostFile(string login, string password, Guid interviewId)
        {
            UserView user = this.GetUserByNameAndPassword();
            if (user == null)
            {
                return this.Json(false, JsonRequestBehavior.AllowGet);
            }
            try
            {
                if (this.Request.Files==null || this.Request.Files.Count == 0 || this.Request.Files[0] == null)
                {
                    return this.Json(false, JsonRequestBehavior.AllowGet);
                }
                using (var memoryStream = new MemoryStream())
                {
                    this.Request.Files[0].InputStream.CopyTo(memoryStream);
                    var data = memoryStream.ToArray();
                    plainFileRepository.StoreInterviewBinaryData(interviewId, this.Request.Files[0].FileName, data);
                }
                return this.Json(true, JsonRequestBehavior.AllowGet);
            }
             catch (Exception ex)
             {
                 this.logger.Fatal("Error on Sync.", ex);
                 this.logger.Fatal("Exception message: " + ex.Message);
                 this.logger.Fatal("Stack: " + ex.StackTrace);

                 return this.Json(false, JsonRequestBehavior.AllowGet);
             }
        }

        //In case of error of type missing or casting error we send correct response.
        [AcceptVerbs(HttpVerbs.Post)]
        [HandleUIException]
        public ActionResult PostPackage(string login, string password, string syncItemContent)
        {
            UserView user = this.GetUserByNameAndPassword();
            if (user == null)
            {
             //   this.logger.Fatal("Stack: " + ex.StackTrace);

                return this.Json(false, JsonRequestBehavior.AllowGet);
            }
            try
            {
                Stream requestStream = this.Request.InputStream;
                requestStream.Seek(0, SeekOrigin.Begin);
                string json = new StreamReader(requestStream).ReadToEnd();
                SyncItem syncItem = null;
                try
                {
                    syncItem = JsonConvert.DeserializeObject<SyncItem>(json,
                                                                       new JsonSerializerSettings
                                                                           {
                                                                               TypeNameHandling =
                                                                                   TypeNameHandling.Objects
                                                                           });
                }
                catch (Exception exc)
                {
                    this.logger.Fatal("Error on Deserialization received stream. Item: ", exc);
                    return this.Json(false, JsonRequestBehavior.AllowGet);
                }

                if (syncItem == null)
                {
                    return this.Json(false, JsonRequestBehavior.AllowGet);
                }

                bool result = this.syncManager.SendSyncItem(syncItem);

                return this.Json(result, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                this.logger.Fatal("Error on Sync.", ex);
                this.logger.Fatal("Exception message: " + ex.Message);
                this.logger.Fatal("Stack: " + ex.StackTrace);

                return this.Json(false, JsonRequestBehavior.AllowGet);
            }
        }

        [AllowAnonymous]
        public ActionResult GetLatestVersion()
        {
            int maxVersion = this.GetLastVersionNumber();

            if (maxVersion > 0)
            {
                var targetToSearchVersions = this.Server.MapPath(this.pathToSearchVersions);
                string path = Path.Combine(targetToSearchVersions, maxVersion.ToString(CultureInfo.InvariantCulture));

                string pathToFile = Path.Combine(path, this.CapiFileName);
                if (System.IO.File.Exists(pathToFile))
                    return this.File(pathToFile, "application/vnd.android.package-archive", this.CapiFileName);
            }

            return null;
        }

        private int GetLastVersionNumber()
        {
            int maxVersion = 0;

            var targetToSearchVersions = this.Server.MapPath(this.pathToSearchVersions);
            if (Directory.Exists(targetToSearchVersions))
            {
                var dirInfo = new DirectoryInfo(targetToSearchVersions);
                foreach (DirectoryInfo directoryInfo in dirInfo.GetDirectories())
                {
                    int value;
                    if (int.TryParse(directoryInfo.Name, out value))
                        if (maxVersion < value)
                            maxVersion = value;
                }
            }

            return maxVersion;
        }


        [AcceptVerbs(HttpVerbs.Post)]
        [HandleUIException]
        [AllowAnonymous]
        public ActionResult CheckNewVersion(string version, string versionCode, string androidId)
        {
            bool isNewVersionExsist = false;

            try
            {
                int versionValue;
                if (int.TryParse(versionCode, out versionValue))
                {
                    int maxVersion = this.GetLastVersionNumber();

                    if (maxVersion != 0 && maxVersion > versionValue)
                    {
                        isNewVersionExsist = true;
                    }
                }
            }
            catch (Exception e)
            {
                this.logger.Error("Error on version check.", e);
            }

            return this.Json(isNewVersionExsist, JsonRequestBehavior.AllowGet);
        }


        //move to filter
        //or change to web API
        private UserView GetUserByNameAndPassword()
        {
            UserView user = null;
            string username = string.Empty;
            string password = string.Empty;

            if (!this.Request.Headers.AllKeys.Contains("Authorization"))
                return null;

            try
            {
                string authHeader = this.Request.Headers["Authorization"];
                char[] delims = { ' ' };
                string[] authHeaderTokens = authHeader.Split(new[] { ' ' });
                if (authHeaderTokens[0].Contains("Basic"))
                {
                    string decodedStr = DecodeFrom64(authHeaderTokens[1]);
                    string[] unpw = decodedStr.Split(new[] { ':' });
                    username = unpw[0];
                    password = unpw[1];
                }
                user = this.GetUser(username, password);

                if (user.isLockedBySupervisor || user.IsLockedByHQ)
                    return null;
            }
            catch (Exception ex)
            {
                this.logger.Fatal("Error on credentials check.", ex);
            }

            return user;
        }

        private static string DecodeFrom64(string encodedData)
        {
            byte[] encodedDataAsBytes = Convert.FromBase64String(encodedData);
            string returnValue = Encoding.ASCII.GetString(encodedDataAsBytes);
            return returnValue;
        }
    }
}