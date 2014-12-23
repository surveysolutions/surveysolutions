using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Flurl;
using Flurl.Http;
using WB.Core.GenericSubdomains.Logging;
using WB.Core.GenericSubdomains.Utils.Services;

namespace WB.Core.GenericSubdomains.Utils.Implementation
{
    public class RestService : IRestService
    {
        private readonly IRestServiceSettings restServiceSettings;
        private readonly ILogger logger;
        private readonly INetworkService networkService;
        private readonly ILocalizationService localizationService;


        public RestService(IRestServiceSettings restServiceSettings, ILogger logger, INetworkService networkService, ILocalizationService localizationService)
        {
            this.restServiceSettings = restServiceSettings;
            this.logger = logger;
            this.networkService = networkService;
            this.localizationService = localizationService;
        }

        private async Task<HttpResponseMessage> ExecuteRequestAsync(string url, Func<FlurlClient, Task<HttpResponseMessage>> request,
            object queryString = null, RestCredentials credentials = null)
        {
            if (this.networkService != null && !this.networkService.IsNetworkEnabled())
            {
                throw new RestException(this.localizationService.GetString("NoNetwork"));
            }

            var restClient = this.restServiceSettings.BaseAddress()
                .AppendPathSegment(url)
                .SetQueryParams(queryString)
                .WithHeader("Accept-Encoding", "gzip,deflate");

            if (credentials != null)
                restClient.WithBasicAuth(credentials.Login, credentials.Password);

            try
            {
                return await request(restClient);
            }
            catch (FlurlHttpTimeoutException ex)
            {
                logger.Error(ex.Message, ex);
                throw new RestException(string.Empty, HttpStatusCode.RequestTimeout, innerException: ex);
            }
            catch (FlurlHttpException ex)
            {
                logger.Error(ex.Message, ex);
                if (ex.Call.Response != null)
                    throw new RestException(string.Empty,
                        statusCode: ex.Call.Response.StatusCode, innerException: ex);

                throw new RestException(message: this.localizationService.GetString("NoConnection"), innerException: ex);
            }
        }

        public async Task<T> GetAsync<T>(string url, object queryString = null, RestCredentials credentials = null)
        {
            return await this.ExecuteRequestAsync(url: url, queryString: queryString, credentials: credentials, request: (client) => client.GetAsync()).ReceiveCompressedJson<T>();
        }

        public async Task GetAsync(string url, object queryString = null, RestCredentials credentials = null)
        {
            await this.ExecuteRequestAsync(url: url, queryString: queryString, credentials: credentials, request: (client) => client.GetAsync());
        }

        public async Task<T> PostAsync<T>(string url, object request = null, RestCredentials credentials = null)
        {
            return await this.ExecuteRequestAsync(url: url, credentials: credentials, request: (client) => client.PostJsonAsync(request)).ReceiveCompressedJson<T>();
        }

        public async Task PostAsync(string url, object request = null, RestCredentials credentials = null)
        {
            await this.ExecuteRequestAsync(url: url, credentials: credentials, request: (client) => client.PostJsonAsync(request));
        }
    }
}
