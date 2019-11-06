using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using WB.UI.Shared.Web.Settings;

namespace WB.UI.Shared.Web.Filters
{
    public class EnforceHttpsHandler : DelegatingHandler
    {
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var isHttpRequest = request.RequestUri.Scheme != Uri.UriSchemeHttps;
            
            if (CoreSettings.IsHttpsRequired)
            {
                var isForwardedFromHttps = request.Headers.Contains("X-Forwarded-Proto")
                                           && request.Headers.GetValues("X-Forwarded-Proto").First()
                                               .Equals("https", StringComparison.OrdinalIgnoreCase);

                if (isHttpRequest && !isForwardedFromHttps)
                {
                    var response = new HttpResponseMessage(HttpStatusCode.Forbidden)
                    {
                        Content = new StringContent("HTTPS Required")
                    };

                    return response;
                }
            }

            if (cancellationToken.IsCancellationRequested)
            {
                return new HttpResponseMessage(HttpStatusCode.InternalServerError);
            }

            return await base.SendAsync(request, cancellationToken);
        }
    }
}
