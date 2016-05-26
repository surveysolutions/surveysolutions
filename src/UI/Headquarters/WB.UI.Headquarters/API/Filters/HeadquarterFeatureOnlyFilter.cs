using System;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http.Controllers;
using System.Web.Http.Filters;

namespace WB.UI.Headquarters.API.Filters
{
    public class HeadquarterFeatureOnlyFilter : IActionFilter
    {
        public Task<HttpResponseMessage> ExecuteActionFilterAsync(HttpActionContext actionContext, CancellationToken cancellationToken,
            Func<Task<HttpResponseMessage>> continuation)
        {
            actionContext.Response = new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.BadRequest,
                RequestMessage = actionContext.ControllerContext.Request,
                ReasonPhrase = "headquarter features are missing with current web site configuration",
                Content = new StringContent("headquarter features are missing with current web site configuration")
            };

            return this.FromResult(actionContext.Response);

        }

        public bool AllowMultiple
        {
            get { return false; }
        }

        private Task<HttpResponseMessage> FromResult(HttpResponseMessage result)
        {
            var source = new TaskCompletionSource<HttpResponseMessage>();
            source.SetResult(result);
            return source.Task;
        }
    }
}