using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Formatting;
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
            var queryString = actionContext.Request.RequestUri.ParseQueryString();
            var interviewId = queryString.Get(InterviewIdQueryString);
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
    }
}
