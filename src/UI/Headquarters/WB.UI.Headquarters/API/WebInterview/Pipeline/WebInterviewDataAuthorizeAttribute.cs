using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Web;
using System.Web.Http.Controllers;
using System.Web.Mvc;
using WB.Enumerator.Native.WebInterview;
using AuthorizeAttribute = System.Web.Http.AuthorizeAttribute;

namespace WB.UI.Headquarters.API.WebInterview.Pipeline
{
    public class WebInterviewDataAuthorizeAttribute : AuthorizeAttribute
    {
        private const string InterviewIdQueryString = "interviewId";

        public override void OnAuthorization(HttpActionContext actionContext)
        {
            try
            {
                CheckPermissions(actionContext);
            }
            catch (InterviewAccessException ie)
            {
                string errorMessage = WB.Enumerator.Native.WebInterview.WebInterview.GetUiMessageFromException(ie);
                actionContext.Response = new HttpResponseMessage(HttpStatusCode.NotAcceptable)
                {
                    Content = new ObjectContent<string>(errorMessage, new JsonMediaTypeFormatter())
                };
            }
        }

        protected override void HandleUnauthorizedRequest(HttpActionContext actionContext)
        {
            try
            {
                CheckPermissions(actionContext);

                actionContext.Response = actionContext.RequestContext.Principal.Identity.IsAuthenticated
                    ? new HttpResponseMessage(HttpStatusCode.Forbidden) 
                    : new HttpResponseMessage(HttpStatusCode.Unauthorized);
            }
            catch (InterviewAccessException ie)
            {
                string errorMessage = WB.Enumerator.Native.WebInterview.WebInterview.GetUiMessageFromException(ie);
                actionContext.Response = new HttpResponseMessage(HttpStatusCode.NotAcceptable)
                {
                    Content = new ObjectContent<string>(errorMessage, new JsonMediaTypeFormatter())
                };
            }
        }

        private void CheckPermissions(HttpActionContext actionContext)
        {
            var interviewId = GetInterviewId(actionContext);
            var isReview = actionContext.Request.Headers.Contains(@"review");

            if (!isReview)
            {
                DependencyResolver.Current.GetService<IWebInterviewAllowService>().CheckWebInterviewAccessPermissions(interviewId);
            }
            else
            {
                DependencyResolver.Current.GetService<IReviewAllowedService>().CheckIfAllowed(Guid.Parse(interviewId));
            }
        }

        private static string GetInterviewId(HttpActionContext actionContext)
        {
            if (actionContext.Request.Method == HttpMethod.Get || actionContext.Request.Method == HttpMethod.Post)
            {
                var queryString = actionContext.Request.RequestUri.ParseQueryString();
                return queryString.Get(InterviewIdQueryString);
            }

            throw new ArgumentException("Cann't resolve interviewId");
        }
    }
}
