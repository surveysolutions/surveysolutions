using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Principal;
using System.Text;
using System.Threading;
using System.Web;
using System.Web.Http.Controllers;
using System.Web.Http.Filters;
using System.Web.Security;
using Main.Core.Entities.SubEntities;
using Microsoft.Practices.ServiceLocation;
using WB.Core.Infrastructure.ReadSide;
using WB.Core.Infrastructure.Transactions;
using WB.Core.SharedKernels.SurveyManagement.Views.User;
using WB.Core.SharedKernels.SurveyManagement.Web.Resources;
using System.Collections.Generic;
using System.Web.Http;
using WB.Core.GenericSubdomains.Portable;

namespace WB.Core.SharedKernels.SurveyManagement.Web.Code
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false)]
    public class ApiBasicAuthAttribute : AuthorizationFilterAttribute
    {
        private readonly Func<string, string, bool> isUserValid;
        private readonly IEnumerable<UserRoles> roles = new UserRoles[] {};
        private bool treatPasswordAsPlain = false;

        private IUserViewFactory userViewFactory
        {
            get { return ServiceLocator.Current.GetInstance<IUserViewFactory>(); }
        }

        private IReadSideStatusService readSideStatusService
        {
            get { return ServiceLocator.Current.GetInstance<IReadSideStatusService>(); }
        }

        private ITransactionManagerProvider TransactionManagerProvider
        {
            get { return ServiceLocator.Current.GetInstance<ITransactionManagerProvider>(); }
        }

        private IPasswordHasher passwordHasher
        {
            get { return ServiceLocator.Current.GetInstance<IPasswordHasher>(); }
        }

        public bool TreatPasswordAsPlain
        {
            get
            {
                return this.treatPasswordAsPlain;
            }
            set
            {
                this.treatPasswordAsPlain = value;
            }
        }

        public ApiBasicAuthAttribute(UserRoles[] roles) : this(Membership.ValidateUser, roles)
        {
        }

        internal ApiBasicAuthAttribute(Func<string, string, bool> isUserValid, IEnumerable<UserRoles> roles)
        {
            this.isUserValid = isUserValid;
            this.roles = roles;
        }

        public override void OnAuthorization(HttpActionContext actionContext)
        {
            if (this.readSideStatusService.AreViewsBeingRebuiltNow())
            {
                this.RespondWithMaintenanceMessage(actionContext);
                return;
            }

            BasicCredentials basicCredentials = ParseCredentials(actionContext);



            if (basicCredentials == null || !this.isUserValid(basicCredentials.Username, treatPasswordAsPlain ? this.passwordHasher.Hash(basicCredentials.Password):basicCredentials.Password))
            {
                this.RespondWithMessageThatUserDoesNotExists(actionContext);
                return;
            }

            this.TransactionManagerProvider.GetTransactionManager().BeginQueryTransaction();
            try
            {
                var userInfo = this.userViewFactory.Load(new UserViewInputModel(UserName: basicCredentials.Username, UserEmail: null));
                if (userInfo == null || userInfo.IsArchived)
                {
                    this.RespondWithMessageThatUserDoesNotExists(actionContext);
                    return;
                }

                if (!userInfo.Roles.Intersect(this.roles).Any())
                {
                    this.RespondWithMessageThatUserIsNoPermittedRole(actionContext);
                    return;
                }
            }
            finally
            {
                this.TransactionManagerProvider.GetTransactionManager().RollbackQueryTransaction();
            }

            var identity = new GenericIdentity(basicCredentials.Username, "Basic");
            var principal = new GenericPrincipal(identity, null);

            Thread.CurrentPrincipal = principal;
            if (HttpContext.Current != null)
            {
                HttpContext.Current.User = principal;
            }

            base.OnAuthorization(actionContext);
        }


        private static BasicCredentials ParseCredentials(HttpActionContext actionContext)
        {
            try
            {
                if (actionContext.Request.Headers.Authorization != null &&
                    actionContext.Request.Headers.Authorization.Scheme == "Basic")
                {
                    string credentials =
                        Encoding.ASCII.GetString(
                            Convert.FromBase64String(actionContext.Request.Headers.Authorization.ToString().Substring(6)));
                    int splitOn = credentials.IndexOf(':');

                    return new BasicCredentials
                    {
                        Username = credentials.Substring(0, splitOn),
                        Password = credentials.Substring(splitOn + 1)
                    };
                }
            }
            catch
            {
            }

            return null;
        }

        internal class BasicCredentials
        {
            public string Username { get; set; }
            public string Password { get; set; }
        }

        private void RespondWithMessageThatUserDoesNotExists(HttpActionContext actionContext)
        {
            actionContext.Response = new HttpResponseMessage(HttpStatusCode.Unauthorized) { ReasonPhrase = string.Format(TabletSyncMessages.InvalidUserFormat, actionContext.Request.RequestUri.GetLeftPart(UriPartial.Authority)) };
            actionContext.Response.Headers.Add("WWW-Authenticate", string.Format("Basic realm=\"{0}\"", actionContext.Request.RequestUri.DnsSafeHost));
        }

        private void RespondWithMaintenanceMessage(HttpActionContext actionContext)
        {
            actionContext.Response = new HttpResponseMessage(HttpStatusCode.ServiceUnavailable) { ReasonPhrase = TabletSyncMessages.Maintenance };
        }

        private void RespondWithMessageThatUserIsNoPermittedRole(HttpActionContext actionContext)
        {
            actionContext.Response = new HttpResponseMessage(HttpStatusCode.Unauthorized) { ReasonPhrase = TabletSyncMessages.InvalidUserRole };
        }

    }
}