using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Flurl.Http;
using Flurl.Http.Configuration;

namespace WB.Core.Infrastructure.Android.Implementation.Services.Rest
{
    internal class RestMessageHandler : FlurlMessageHandler
    {
        private readonly CancellationToken token;

        public RestMessageHandler(CancellationToken token)
            : base(FlurlHttp.Configuration.HttpClientFactory.CreateMessageHandler())
        {
            this.token = token;
        }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            return base.SendAsync(request, token == default(CancellationToken) ? cancellationToken : token);
        }
    }
}