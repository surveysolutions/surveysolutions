using System.Net.Http;
using System.Threading;

using Flurl;
using Flurl.Http;

using WB.Core.GenericSubdomains.Portable.Implementation;
using WB.Core.GenericSubdomains.Portable.Rest;
using WB.Core.GenericSubdomains.Portable.Services;

namespace WB.Core.GenericSubdomains.Native.Rest
{
    public class FlurlRestClientProvider : IRestClientProvider
    {
        public IRestClient GetRestClient(string url, object queryString, RestCredentials credentials, IRestServiceSettings restServiceSettings, CancellationToken token)
        {
            Url fullUrl = restServiceSettings.Endpoint
                .AppendPathSegment(url)
                .SetQueryParams(queryString);

            FlurlClient restClient = fullUrl
                .ConfigureHttpClient(http => new HttpClient(new RestMessageHandler(token)))
                .WithTimeout(restServiceSettings.Timeout)
                .WithHeader("Accept-Encoding", "gzip,deflate");

            if (credentials != null)
                restClient.WithBasicAuth(credentials.Login, credentials.Password);

            return new FlurlRestClient(restClient);
        }
    }
}