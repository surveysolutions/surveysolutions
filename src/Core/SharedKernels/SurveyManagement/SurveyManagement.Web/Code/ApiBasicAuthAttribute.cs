using System;
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
using WB.Core.SharedKernels.SurveyManagement.Views.User;
using WB.Core.SharedKernels.SurveyManagement.Web.Properties;
using WB.Core.SharedKernels.SurveyManagement.Web.Resources;

namespace WB.Core.SharedKernels.SurveyManagement.Web.Code
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false)]
    public class ApiBasicAuthAttribute : AuthorizationFilterAttribute
    {
        private readonly Func<string, string, bool> isUserValid;

        private IUserViewFactory userViewFactory
        {
            get { return ServiceLocator.Current.GetInstance<IUserViewFactory>(); }
        }

        private IReadSideStatusService readSideStatusService
        {
            get { return ServiceLocator.Current.GetInstance<IReadSideStatusService>(); }
        }

        internal static string[] SplitString(string original)
        {
            return string.IsNullOrEmpty(original) ? new string[0] : original.Split(',');
        }

        public ApiBasicAuthAttribute() : this(Membership.ValidateUser)
        {
        }

        internal ApiBasicAuthAttribute(Func<string, string, bool> isUserValid)
        {
            this.isUserValid = isUserValid;
        }

        public override void OnAuthorization(HttpActionContext actionContext)
        {
            if (this.readSideStatusService.AreViewsBeingRebuiltNow())
            {
                this.ResponseMaintenanceMessage(actionContext);
                return;
            }

            BasicCredentials basicCredentials = ParseCredentials(actionContext);
            if (basicCredentials == null || !this.isUserValid(basicCredentials.Username, basicCredentials.Password))
            {
                this.RespondWithMessageThatUserDoesNotExists(actionContext);
                return;
            }

            var userInfo = this.userViewFactory.Load(new UserViewInputModel(UserName: basicCredentials.Username, UserEmail: null));
            if (userInfo == null || !userInfo.Roles.Contains(UserRoles.Operator))
            {
                this.RespondWithMessageThatUserIsNotAnInterviewer(actionContext);
                return;
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
            actionContext.Response = new HttpResponseMessage(HttpStatusCode.Unauthorized) { ReasonPhrase = string.Format(InterviewerSyncStrings.InvalidUserFormat, actionContext.Request.RequestUri.GetLeftPart(UriPartial.Authority)) };
            actionContext.Response.Headers.Add("WWW-Authenticate", string.Format("Basic realm=\"{0}\"", actionContext.Request.RequestUri.DnsSafeHost));
        }

        private void ResponseMaintenanceMessage(HttpActionContext actionContext)
        {
            actionContext.Response = new HttpResponseMessage(HttpStatusCode.ServiceUnavailable) { ReasonPhrase = InterviewerSyncStrings.Maintenance };
        }

        private void RespondWithMessageThatUserIsNotAnInterviewer(HttpActionContext actionContext)
        {
            actionContext.Response = new HttpResponseMessage(HttpStatusCode.Unauthorized) { ReasonPhrase = InterviewerSyncStrings.InvalidUserRole };
        }
    }
}