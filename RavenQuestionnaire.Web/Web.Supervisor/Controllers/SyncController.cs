using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Web.Mvc;
using System.Web.Security;
using Main.Core.Entities.SubEntities;
using Main.Core.Events;
using Main.Core.Export;
using Main.Core.View;
using Main.Core.View.User;
using Newtonsoft.Json;
using SynchronizationMessages.CompleteQuestionnaire;
using SynchronizationMessages.Synchronization;
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
        public bool Handshake(string login, string password, string clientID, string LastSyncID)
        {
            var user = GetUser(login, password);
            if (user == null)
                throw new HttpStatusException(HttpStatusCode.Forbidden);
            return true;
        }


        [AcceptVerbs(HttpVerbs.Post)]
        [HandleUIException]
        public ActionResult GetSyncPackage(string aRKey,string login, string password)
        {
            var user = GetUser(login, password);
            if (user == null)
                throw new HttpStatusException(HttpStatusCode.Forbidden);

            Guid key;
            if (!Guid.TryParse(aRKey, out key))
            {
                return null; //todo: return correct description
            }

            try
            {
                var package = this.syncManager.ReceiveSyncPackage(null, key);
                return Json(package, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                logger.Fatal("Error on sync", ex);
                return null;
            }
        }

        [AcceptVerbs(HttpVerbs.Post)]
        [HandleUIException]
        public JsonResult GetARKeys(string login, string password)
        {
            var user = GetUser(login, password);
            if (user == null)
                throw new HttpStatusException(HttpStatusCode.Forbidden);

            return Json(this.GetListOfAR(user.PublicKey));
        }

        private SyncItemsMetaContainer GetListOfAR(Guid userId)
        {
           
            var result = new SyncItemsMetaContainer();

            try
            {
                var package = this.syncManager.GetAllARIds(userId);
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
