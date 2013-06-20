using System;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using Main.Core.Entities.SubEntities;
using Main.Core.View;
using Main.Core.View.User;
using Newtonsoft.Json;
using WB.Core.SharedKernel.Logger;
using WB.Core.SharedKernel.Structures.Synchronization;
using WB.Core.Synchronization;
using WB.UI.Shared.Web.Exceptions;
using WB.UI.Shared.Web.Filters;

namespace Web.Supervisor.Controllers
{
    public class SyncController : AsyncController
    {
        private readonly ILog logger;
        private readonly WB.Core.Synchronization.SyncManager.ISyncManager syncManager;
        private readonly IViewFactory<UserViewInputModel, UserView> viewFactory;

        public SyncController(WB.Core.Synchronization.SyncManager.ISyncManager syncManager, ILog logger,
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

        [AcceptVerbs(HttpVerbs.Post)]
        [HandleUIException]
        public ActionResult Handshake(string login, string password, string clientId, string androidId, Guid? clientRegistrationId)
        {
            var user = GetUser(login, password);
            if (user == null)
                throw new HttpStatusException(HttpStatusCode.Forbidden);

            var package = new HandshakePackage();

            Guid key;
            if (!Guid.TryParse(clientId, out key))
            {
                package.IsErrorOccured = true;
                package.ErrorMessage = "Client Identifier was not provided";
                
            }
            else
            {
                ClientIdentifier identifier = new ClientIdentifier();
                identifier.ClientDeviceKey = androidId;
                identifier.ClientInstanceKey = key;
                identifier.ClientVersionIdentifier = "unknown";
                identifier.ClientRegistrationKey = clientRegistrationId;
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
        public ActionResult GetSyncPackage(string aRKey, string aRSequence, Guid clientRegistrationId, string login, string password)
        {
            var user = GetUser(login, password);
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

            long sequence;
            if (!long.TryParse(aRSequence, out sequence))
            {
                package.IsErrorOccured = true;
                package.ErrorMessage = "Invalid sequence identifier";
                return Json(package, JsonRequestBehavior.AllowGet);
            }
            
            try
            {
                package = this.syncManager.ReceiveSyncPackage(clientRegistrationId, key, sequence);
                return Json(package, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                logger.Fatal("Error on sync", ex);
                package.IsErrorOccured = true;
                package.ErrorMessage = "Error occured. Try later.";
                return Json(package, JsonRequestBehavior.AllowGet);
            }
        }

        [AcceptVerbs(HttpVerbs.Post)]
        [HandleUIException]
        public JsonResult GetARKeys(string login, string password, Guid clientRegistrationKey)
        {
            var user = GetUser(login, password);
            if (user == null)
                throw new HttpStatusException(HttpStatusCode.Forbidden);

            if (clientRegistrationKey == Guid.Empty)
                throw new HttpException("Incorrect parameter set.");

            return Json(this.GetListOfAR(user.PublicKey, clientRegistrationKey));
        }

        private SyncItemsMetaContainer GetListOfAR(Guid userId, Guid clientRegistrationKey)
        {
            var result = new SyncItemsMetaContainer();

            try
            {
                var package = this.syncManager.GetAllARIdsWithOrder(userId, clientRegistrationKey);
                result.ARId = package.ToList();
            }
            catch (Exception ex)
            {
                logger.Fatal("Error on retrieving the list of AR on sync. ", ex);
                logger.Fatal(ex.Message);
                logger.Fatal(ex.StackTrace);
            }

            return result;
        }
        [AcceptVerbs(HttpVerbs.Post)]
        [HandleUIException]
        public ActionResult PostPackage(string login, string password, string syncItemContent)
        {
            var user = GetUser(login, password);
            if (user == null)
                throw new HttpStatusException(HttpStatusCode.Forbidden);

            try
            {
                SyncItem syncItem = null;
                try
                {
                    syncItem = JsonConvert.DeserializeObject<SyncItem>(syncItemContent,
                                                                           new JsonSerializerSettings
                                                                               {
                                                                                   TypeNameHandling =
                                                                                       TypeNameHandling.Objects
                                                                               });
                    
                }
                catch (Exception exc)
                {
                    logger.Fatal("Error on Deserialization received stream. Item: ", exc);
                    throw;
                }

                if (syncItem == null)
                {
                    return Json(false, JsonRequestBehavior.AllowGet);
                }

                return Json(this.syncManager.SendSyncItem(syncItem), JsonRequestBehavior.AllowGet);
                
            }
            catch (Exception ex)
            {
                logger.Fatal("Error on Sync.", ex);
                logger.Fatal("Exception message: " + ex.Message);
                logger.Fatal("Stack: " + ex.StackTrace);

                return Json(false, JsonRequestBehavior.AllowGet);
            }
        }


    }
}
