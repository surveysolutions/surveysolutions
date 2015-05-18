using System;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using WB.Core.GenericSubdomains.Portable.Properties;
using WB.Core.GenericSubdomains.Utils.Implementation;
using WB.Core.GenericSubdomains.Utils.Services;

namespace WB.Core.GenericSubdomains.Portable.Rest
{
    public class RestService : IRestService
    {
        private readonly IRestServiceSettings restServiceSettings;
        private readonly ILogger logger;
        private readonly INetworkService networkService;
        private readonly IRestServicePointManager servicePointManager;
        private readonly IRestClientProvider restClientProvider;

        public RestService(
            IRestServiceSettings restServiceSettings, 
            ILogger logger, 
            INetworkService networkService, 
            IRestServicePointManager servicePointManager, 
            IRestClientProvider restClientProvider)
        {
            this.restServiceSettings = restServiceSettings;
            this.logger = logger;
            this.networkService = networkService;
            this.servicePointManager = servicePointManager;
            this.restClientProvider = restClientProvider;

            if (this.restServiceSettings.AcceptUnsignedSslCertificate)
                this.AcceptUnsignedSslCertificate();
        }

        private async Task<HttpResponseMessage> ExecuteRequestAsync(
            string url, 
            Func<IRestClient, Task<HttpResponseMessage>> request, 
            CancellationToken token,
            object queryString = null, 
            RestCredentials credentials = null)
        {
            if (this.networkService != null && !this.networkService.IsNetworkEnabled())
            {
                throw new RestException(Resources.NoNetwork);
            }

            IRestClient client = restClientProvider.GetRestClient(url, queryString, credentials, this.restServiceSettings, token);

            try
            {
                return await request(client);
            }
            catch (RestHttpTimeoutException ex)
            {
                throw new RestException(message: Resources.Timeout, statusCode: HttpStatusCode.RequestTimeout, innerException: ex);
            }
            catch (RestHttpException ex)
            {
                this.logger.Error(string.Format("Request to '{0}'. QueryParams: {1} failed. ", client.GetFullUrl(), queryString), ex);

                if (!string.IsNullOrWhiteSpace(ex.ReasonPhrase))
                    throw new RestException(ex.ReasonPhrase, statusCode: ex.StatusCode, innerException: ex);

                throw new RestException(message: Resources.NoConnection, innerException: ex);
            }
            catch (WebException ex)
            {
                this.logger.Error(string.Format("Request to '{0}'. QueryParams: {1} failed. ", client.GetFullUrl(), queryString), ex);
                throw new RestException(message: Resources.NoConnection, innerException: ex);
            }
        }

        private void AcceptUnsignedSslCertificate()
        {
            this.servicePointManager.AcceptUnsignedSslCertificate();
        }

        public async Task<T> GetAsync<T>(string url, object queryString = null, RestCredentials credentials = null)
        {
            return await this.GetAsync<T>(url: url, queryString: queryString, credentials: credentials, token: default(CancellationToken));
        }

        public async Task GetAsync(string url, object queryString = null, RestCredentials credentials = null)
        {
            await this.GetAsync(url: url, queryString: queryString, credentials: credentials, token: default(CancellationToken));
        }

        public async Task<T> PostAsync<T>(string url, object request = null, RestCredentials credentials = null)
        {
            return await this.PostAsync<T>(url: url, credentials: credentials, request: request, token: default(CancellationToken));
        }

        public async Task PostAsync(string url, object request = null, RestCredentials credentials = null)
        {
            await this.PostAsync(url: url, credentials: credentials, request: request, token: default(CancellationToken));
        }

        public async Task<T> GetAsync<T>(string url, CancellationToken token, object queryString = null, RestCredentials credentials = null)
        {
            return await this.ExecuteRequestAsync(url: url, queryString: queryString, credentials: credentials, request: (client) => client.GetAsync(), token: token)
                .ReceiveCompressedJsonAsync<T>();
        }

        public async Task GetAsync(string url, CancellationToken token, object queryString = null, RestCredentials credentials = null)
        {
            await this.ExecuteRequestAsync(url: url, queryString: queryString, credentials: credentials, request: (client) => client.GetAsync(), token: token);
        }

        public async Task<T> PostAsync<T>(string url, CancellationToken token, object request = null, RestCredentials credentials = null)
        {
            return await this.ExecuteRequestAsync(url: url, credentials: credentials, request: (client) => client.PostJsonAsync(request), token: token)
                .ReceiveCompressedJsonAsync<T>();
        }

        public async Task PostAsync(string url, CancellationToken token, object request = null, RestCredentials credentials = null)
        {
            await this.ExecuteRequestAsync(url: url, credentials: credentials, request: (client) => client.PostJsonAsync(request), token: token);
        }

        public async Task<T> GetWithProgressAsync<T>(string url, CancellationToken token, Action<decimal> progressPercentage, object queryString = null,
            RestCredentials credentials = null)
        {
            return await this.ExecuteRequestAsync(url: url, queryString: queryString, credentials: credentials, request: (client) => client.GetAsync(), token: token)
                .ReceiveCompressedJsonWithProgressAsync<T>(token: token, progressPercentage: progressPercentage);
        }

        public async Task<T> PostWithProgressAsync<T>(string url, CancellationToken token, Action<decimal> progressPercentage, object request = null,
            RestCredentials credentials = null)
        {
            return await this.ExecuteRequestAsync(url: url, credentials: credentials, request: (client) => client.PostJsonAsync(request), token: token)
                .ReceiveCompressedJsonWithProgressAsync<T>(token: token, progressPercentage: progressPercentage);
        }
    }
}
