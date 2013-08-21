using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Web.Hosting;
using System.Web.Mvc;
using System.Web.Security;
using Main.Core.Entities.SubEntities;
using Main.Core.View;
using Main.Core.View.User;
using Newtonsoft.Json;
using WB.Core.GenericSubdomains.Logging;
using WB.Core.SharedKernel.Structures.Synchronization;
using WB.Core.Synchronization;
using WB.UI.Shared.Web.Exceptions;
using WB.UI.Shared.Web.Filters;

namespace Web.Supervisor.Controllers
{
    public class SyncController : AsyncController
    {
        private readonly ILogger logger;
        private readonly ISyncManager syncManager;
        private readonly IViewFactory<UserViewInputModel, UserView> viewFactory;

        private string CapiFileName = "wbcapi.apk";

        private string pathToSearchVersions = ("~/App_Data/Capi");

        public SyncController(ISyncManager syncManager, ILogger logger,
            IViewFactory<UserViewInputModel, UserView> viewFactory)
        {
            this.syncManager = syncManager;
            this.logger = logger;
            this.viewFactory = viewFactory;
        }


        protected UserView GetUser(string login, string password)
        {

            if (Membership.ValidateUser(login, password))
            {
                if (Roles.IsUserInRole(login, UserRoles.Operator.ToString()))
                {
                    return
                        this.viewFactory.Load(new UserViewInputModel(login, null));

                }
            }
            return null;
        }

        //In case of error of type missing or casting error we send correct response.
        [AcceptVerbs(HttpVerbs.Post)]
        [HandleUIException]
        public ActionResult Handshake(string clientId, string androidId, Guid? clientRegistrationId)
        {
            var user = GetUserByNameAndPassword();
            if (user == null)
                throw new HttpStatusException(HttpStatusCode.Forbidden);

            var package = new HandshakePackage();

            Guid key;
            if (!Guid.TryParse(clientId, out key))
            {
                package.IsErrorOccured = true;
                package.ErrorMessage = "Client Identifier was not provided.";
            }
            else
            {
                ClientIdentifier identifier = new ClientIdentifier();
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
                    package.ErrorMessage = "Error occured on sync. Try later.";    
                }
            }
            return Json(package, JsonRequestBehavior.AllowGet);
        }

        
        [AcceptVerbs(HttpVerbs.Post)]
        [HandleUIException]
        public ActionResult InitPulling(string clientRegistrationId)
        {
            var user = GetUserByNameAndPassword();
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
            
            return Json(package, JsonRequestBehavior.AllowGet);
        }

        //In case of error of type missing or casting error we send correct response.
        [AcceptVerbs(HttpVerbs.Post)]
        [HandleUIException]
        public ActionResult GetSyncPackage(string aRKey, string aRSequence, string clientRegistrationId)
        {
            var user = GetUserByNameAndPassword();
            if (user == null)
                throw new HttpStatusException(HttpStatusCode.Forbidden);

            var package = new SyncPackage();

            Guid key;
            if (!Guid.TryParse(aRKey, out key))
            {
                package.IsErrorOccured = true;
                package.ErrorMessage = "Invalid object identifier";
                return Json(package, JsonRequestBehavior.AllowGet);
            }

            Guid clientRegistrationKey;
            if (!Guid.TryParse(clientRegistrationId, out clientRegistrationKey))
            {
                package.IsErrorOccured = true;
                package.ErrorMessage = "Invalid device identifier";
                return Json(package, JsonRequestBehavior.AllowGet);
            }

            long sequence;
            if (!long.TryParse(aRSequence, out sequence))
            {
                package.IsErrorOccured = true;
                package.ErrorMessage = "Invalid sequence identifier";
                return Json(package, JsonRequestBehavior.AllowGet);
            }
            
            try
            {
                package = this.syncManager.ReceiveSyncPackage(clientRegistrationKey, key, sequence);
                
            }
            catch (Exception ex)
            {
                logger.Fatal("Error on sync", ex);
                logger.Fatal(ex.StackTrace);

                package.IsErrorOccured = true;
                package.ErrorMessage = "Error occured. Try later.";
            }

            return Json(package, JsonRequestBehavior.AllowGet);

        }

        //In case of error of type missing or casting error we send correct response.
        [AcceptVerbs(HttpVerbs.Post)]
        [HandleUIException]
        public JsonResult GetARKeys(string clientRegistrationId, string sequence)
        {
            var user = GetUserByNameAndPassword();
            if (user == null)
                throw new HttpStatusException(HttpStatusCode.Forbidden);

            Guid clientRegistrationKey;
            if (!Guid.TryParse(clientRegistrationId, out clientRegistrationKey))
            {
                var result = new SyncItemsMetaContainer(); 
                result.IsErrorOccured = true;
                return Json(result, JsonRequestBehavior.AllowGet);
            }

            if (clientRegistrationKey == Guid.Empty)
            {
                var result = new SyncItemsMetaContainer();
                result.IsErrorOccured = true;
                return Json(result, JsonRequestBehavior.AllowGet);
            }

            if (string.IsNullOrWhiteSpace(sequence))
                sequence = "0";

            long clientSequence;
            if (!long.TryParse(sequence, out clientSequence))
            {
                var result = new SyncItemsMetaContainer(); 
                result.IsErrorOccured = true;
                return Json(result, JsonRequestBehavior.AllowGet);
            }

            return Json(this.GetListOfAR(user.PublicKey, clientRegistrationKey, clientSequence), JsonRequestBehavior.AllowGet);
        }

        private SyncItemsMetaContainer GetListOfAR(Guid userId, Guid clientRegistrationKey, long clientSequence)
        {
            var result = new SyncItemsMetaContainer();

            try
            {
                var package = this.syncManager.GetAllARIdsWithOrder(userId, clientRegistrationKey, clientSequence);
                result.ARId = package.ToList();
            }
            catch (Exception ex)
            {
                logger.Fatal("Error on retrieving the list of AR on sync. ", ex);
                logger.Fatal(ex.Message);
                logger.Fatal(ex.StackTrace);

                result.ErrorMessage = "Server error occured.";
                result.IsErrorOccured = true;
            }

            return result;
        }

        //In case of error of type missing or casting error we send correct response.
        [AcceptVerbs(HttpVerbs.Post)]
        [HandleUIException]
        public ActionResult PostPackage(string login, string password, string syncItemContent)
        {
            var user = GetUserByNameAndPassword();
            if (user == null)
                throw new HttpStatusException(HttpStatusCode.Forbidden);

            try
            {
                Stream requestStream = Request.InputStream;
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
                    logger.Fatal("Error on Deserialization received stream. Item: ", exc);
                    return Json(false, JsonRequestBehavior.AllowGet);
                }

                if (syncItem == null)
                {
                    return Json(false, JsonRequestBehavior.AllowGet);
                }

                var result = this.syncManager.SendSyncItem(syncItem);

                return Json(result, JsonRequestBehavior.AllowGet);

            }
            catch (Exception ex)
            {
                logger.Fatal("Error on Sync.", ex);
                logger.Fatal("Exception message: " + ex.Message);
                logger.Fatal("Stack: " + ex.StackTrace);

                return Json(false, JsonRequestBehavior.AllowGet);
            }
        }

        [AllowAnonymous]
        public ActionResult GetLatestVersion()
        {
            int maxVersion = GetLastVersionNumber();
            
            if (maxVersion != 0)
            {
                string path = Path.Combine(Server.MapPath(pathToSearchVersions), maxVersion.ToString(CultureInfo.InvariantCulture));

                string pathToFile = Path.Combine(path, CapiFileName);
                if (System.IO.File.Exists(pathToFile))
                    return File(pathToFile, "application/vnd.android.package-archive", CapiFileName);
            }
            
            return null;
        }

        private int GetLastVersionNumber()
        {
            int maxVersion = 0;

            var targetToSearchVersions = Server.MapPath(pathToSearchVersions);
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
            var isNewVersionExsist = false;

            try
            {
                int versionValue;
                if (int.TryParse(versionCode, out versionValue))
                {
                    int maxVersion = GetLastVersionNumber();

                    if (maxVersion != 0 && maxVersion > versionValue)
                    {
                        isNewVersionExsist = true;
                    }
                }
            }
            catch (Exception e)
            {
                logger.Error("Error on version check.", e);
            }

            return Json(isNewVersionExsist, JsonRequestBehavior.AllowGet);
        }


        //move to filter
        //or change to web API
        private UserView GetUserByNameAndPassword()
        {
            UserView user = null;
            var username = string.Empty;
            var password = string.Empty;

            if (!Request.Headers.AllKeys.Contains("Authorization")) 
                return null;

            try
            {
                string authHeader = Request.Headers["Authorization"];
                char[] delims = {' '};
                string[] authHeaderTokens = authHeader.Split(new char[] {' '});
                if (authHeaderTokens[0].Contains("Basic"))
                {
                    string decodedStr = DecodeFrom64(authHeaderTokens[1]);
                    string[] unpw = decodedStr.Split(new char[] {':'});
                    username = unpw[0];
                    password = unpw[1];
                }
                user = GetUser(username, password);
            }
            catch (Exception ex)
            {
                logger.Fatal("Error on credentials check.", ex);
            }

            return user;
        }

        private static string DecodeFrom64(string encodedData)
        {
            byte[] encodedDataAsBytes = System.Convert.FromBase64String(encodedData);
            string returnValue = System.Text.Encoding.ASCII.GetString(encodedDataAsBytes);
            return returnValue;
        }
    }
}
