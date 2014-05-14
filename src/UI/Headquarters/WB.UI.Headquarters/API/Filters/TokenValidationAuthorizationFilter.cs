using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http.Controllers;
using System.Web.Http.Filters;
using WB.UI.Headquarters.API.Attributes;

namespace WB.UI.Headquarters.API.Filters
{
    public class TokenValidationAuthorizationFilter : IAuthorizationFilter
    {
        const string Apikey = "Authorization";
        private readonly ITokenVerifier tokenVerifier;

        public TokenValidationAuthorizationFilter(ITokenVerifier tokenVerifier)
        {
            this.tokenVerifier = tokenVerifier;
        }

        public bool AllowMultiple 
        {
            get { return false; }
        }

        public Task<HttpResponseMessage> ExecuteAuthorizationFilterAsync(HttpActionContext actionContext, CancellationToken cancellationToken, Func<Task<HttpResponseMessage>> continuation)
        {
            try
            {
                string token = actionContext.Request.Headers.GetValues(Apikey).FirstOrDefault();

                if (!this.tokenVerifier.IsTokenValid(token))
                {
                    actionContext.Response = new HttpResponseMessage
                    {
                        StatusCode = HttpStatusCode.Forbidden,
                        RequestMessage = actionContext.ControllerContext.Request,
                        ReasonPhrase = "Invalid token",
                        Content = new StringContent("Invalid token")
                    };
                    return this.FromResult(actionContext.Response);
                }
            }
            catch (Exception)
            {
                actionContext.Response = new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.BadRequest,
                    RequestMessage = actionContext.ControllerContext.Request,
                    ReasonPhrase = "Authorization token is missing",
                    Content = new StringContent("Authorization token is missing")
                };

                return this.FromResult(actionContext.Response);
            }

            return continuation();
        }


        private Task<HttpResponseMessage> FromResult(HttpResponseMessage result)
        {
            var source = new TaskCompletionSource<HttpResponseMessage>();
            source.SetResult(result);
            return source.Task;
        }
    }
}