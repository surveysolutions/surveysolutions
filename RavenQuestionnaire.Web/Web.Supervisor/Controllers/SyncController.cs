using System;
using System.IO;
using System.Net;
using System.Text;
using System.Web.Mvc;
using System.Web.Security;
using DataEntryClient.SycProcessFactory;
using Main.Core.Entities.SubEntities;
using Main.Core.Events;
using Main.Core.Export;
using Main.Core.View;
using Main.Core.View.User;
using Newtonsoft.Json;
using SynchronizationMessages.CompleteQuestionnaire;
using SynchronizationMessages.Synchronization;
using WB.Core.SharedKernel.Logger;
using WB.Core.Synchronization;
using WB.UI.Shared.Web.Exceptions;
using WB.UI.Shared.Web.Filters;

namespace Web.Supervisor.Controllers
{
    public class SyncController : AsyncController
    {
        #region Fields

        /// <summary>
        /// The syncs process factory
        /// </summary>
        private readonly ISyncProcessFactory syncProcessFactory;

        /// <summary>
        /// View repository
        /// </summary>
        private readonly IViewRepository viewRepository;

        /// <summary>
        /// The logger.
        /// </summary>
        private readonly ILog logger;

        /// <summary>
        /// The logger.
        /// </summary>
        private readonly WB.Core.Synchronization.SyncManager.ISyncManager syncManager;

        #endregion

        public SyncController(
            IViewRepository viewRepository, ISyncProcessFactory syncProcessFactory, 
            WB.Core.Synchronization.SyncManager.ISyncManager syncManager, ILog logger)
        {
            
            this.viewRepository = viewRepository;
            this.syncProcessFactory = syncProcessFactory;
            this.syncManager = syncManager;

            this.logger = logger;
        }


        protected UserView GetUser(string login, string password)
        {

            if (Membership.ValidateUser(login, password))
            {
                if (Roles.IsUserInRole(login, UserRoles.Operator.ToString()))
                {
                    return
                        this.viewRepository.Load<UserViewInputModel, UserView>(new UserViewInputModel(login, null));

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
        public ActionResult GetSyncPackage(string aRKey, string rootType, string login, string password)
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
                var package = this.syncManager.ReceiveSyncPackage(null, key, rootType);
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
        public bool PostPackage(string login, string password)
        {
            var user = GetUser(login, password);
            if (user == null)
                throw new HttpStatusException(HttpStatusCode.Forbidden);
            
            if (Request.Files == null || Request.Files.Count == 0)
                return false;
            try
            {
                SyncItem syncItem = null;
                try
                {
                    using (var reader = new StreamReader(Request.InputStream, Encoding.UTF8))
                    {
                        syncItem =  JsonConvert.DeserializeObject<SyncItem>(
                            reader.ReadToEnd(), new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.Objects });
                    }
                }
                catch (Exception exc)
                {
                    logger.Fatal("Error on Deserialization received stream. Item: ", exc);
                    throw;
                }

                if (syncItem == null)
                {
                    return false;
                }

                return this.syncManager.SendSyncItem(syncItem);
                
            }
            catch (Exception ex)
            {
                logger.Fatal("Error on Sync.", ex);
                logger.Fatal("Exception message: " + ex.Message);
                logger.Fatal("Stack: " + ex.StackTrace);

                return false;
            }
        }


    }
}
