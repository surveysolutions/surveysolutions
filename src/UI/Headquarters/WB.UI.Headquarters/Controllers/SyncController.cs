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
using WB.Core.Synchronization;
using WB.UI.Shared.Web.Exceptions;
using WB.UI.Shared.Web.Filters;

namespace WB.UI.Headquarters.Controllers
{
    public class SyncController : AsyncController
    {
        private readonly ILogger logger;
        private readonly ISyncManager syncManager;
        private readonly IViewFactory<UserViewInputModel, UserView> viewFactory;

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
            UserView user = this.GetUserByNameAndPassword();
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
        public ActionResult PostPackage(string syncItemContent)
        {
            try
            {
                Stream requestStream = this.Request.InputStream;
                requestStream.Seek(0, SeekOrigin.Begin);
                string json = new StreamReader(requestStream).ReadToEnd();
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
                char[] delims = {' '};
                string[] authHeaderTokens = authHeader.Split(new[] {' '});
                if (authHeaderTokens[0].Contains("Basic"))
                {
                    string decodedStr = DecodeFrom64(authHeaderTokens[1]);
                    string[] unpw = decodedStr.Split(new[] {':'});
                    username = unpw[0];
                    password = unpw[1];
                }
                user = this.GetUser(username, password);
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