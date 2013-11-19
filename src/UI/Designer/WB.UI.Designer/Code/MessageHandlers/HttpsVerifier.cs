namespace WB.UI.Designer.Code.MessageHandlers
{
    using System;
    using System.Net;
    using System.Net.Http;
    using System.Threading;
    using System.Threading.Tasks;

    public class HttpsVerifier : DelegatingHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, 
            CancellationToken cancellationToken)
        {
            if (request.RequestUri.Scheme == Uri.UriSchemeHttps)
                return base.SendAsync(request, cancellationToken);


            HttpResponseMessage response= request.CreateErrorResponse(HttpStatusCode.BadRequest, 
                    "HTTPS is required");

            var taskCompletionSource = new TaskCompletionSource<HttpResponseMessage>();
            taskCompletionSource.SetResult(response);
            return taskCompletionSource.Task;
        }
    }

    
}