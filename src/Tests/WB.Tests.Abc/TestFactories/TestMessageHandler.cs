using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace WB.Tests.Abc.TestFactories
{
    public class TestMessageHandler : HttpMessageHandler
    {
        private readonly Dictionary<string, string> responseHeaders;

        public TestMessageHandler(Dictionary<string, string> responseHeaders = null)
        {
            this.responseHeaders = responseHeaders;
        }

        public List<HttpRequestMessage> ExecutedRequests { get; } = new List<HttpRequestMessage>();

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            this.ExecutedRequests.Add(request);
            var response = new HttpResponseMessage(HttpStatusCode.OK);
            if (this.responseHeaders != null)
            {
                foreach (var header in this.responseHeaders)
                {
                    response.Headers.TryAddWithoutValidation(header.Key, header.Value);
                }
            }
            return Task.FromResult(response);
        }
    }
}
