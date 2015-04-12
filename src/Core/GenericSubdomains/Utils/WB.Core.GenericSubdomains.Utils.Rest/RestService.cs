using System;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Flurl;
using Flurl.Http;
using WB.Core.GenericSubdomains.Utils.Implementation;
using WB.Core.GenericSubdomains.Utils.Rest.Properties;
using WB.Core.GenericSubdomains.Utils.Services;

namespace WB.Core.GenericSubdomains.Utils.Rest
{
    public class RestService : IRestService
    {
        private readonly IRestServiceSettings restServiceSettings;
        private readonly ILogger logger;
        private readonly INetworkService networkService;

        public RestService(IRestServiceSettings restServiceSettings, ILogger logger, INetworkService networkService)
        {
            this.restServiceSettings = restServiceSettings;
            this.logger = logger;
            this.networkService = networkService;

            if (this.restServiceSettings.AcceptUnsignedSslCertificate)
                AcceptUnsignedSslCertificate();
        }

        private async Task<HttpResponseMessage> ExecuteRequestAsync(string url, Func<FlurlClient, Task<HttpResponseMessage>> request, CancellationToken token,
            object queryString = null, RestCredentials credentials = null)
        {
            if (this.networkService != null && !this.networkService.IsNetworkEnabled())
            {
                throw new RestException(Resources.NoNetwork);
            }

            var fullUrl = this.restServiceSettings.Endpoint
                .AppendPathSegment(url)
                .SetQueryParams(queryString);

            var restClient = fullUrl
                .ConfigureHttpClient(http => new HttpClient(new RestMessageHandler(token)))
                .WithTimeout(this.restServiceSettings.Timeout)
                .WithHeader("Accept-Encoding", "gzip,deflate");

            if (credentials != null)
                restClient.WithBasicAuth(credentials.Login, credentials.Password);

            try
            {
                return await request(restClient);
            }
            catch (FlurlHttpTimeoutException ex)
            {
                throw new RestException(message: Resources.Timeout, statusCode: HttpStatusCode.RequestTimeout, innerException: ex);
            }
            catch (FlurlHttpException ex)
            {
                this.logger.Error(string.Format("Request to '{0}'. QueryParams: {1} failed. ", fullUrl, fullUrl.QueryParams), ex);

                if (ex.Call.Response != null)
                    throw new RestException(ex.Call.Response.ReasonPhrase, statusCode: ex.Call.Response.StatusCode,
                        innerException: ex);

                throw new RestException(message: Resources.NoConnection, innerException: ex);
            }
            catch (WebException ex)
            {
                this.logger.Error(string.Format("Request to '{0}'. QueryParams: {1} failed. ", fullUrl, fullUrl.QueryParams), ex);
                throw new RestException(message: Resources.NoConnection, innerException: ex);
            }
        }

        private static void AcceptUnsignedSslCertificate()
        {
            ServicePointManager.ServerCertificateValidationCallback += (sender, cert, chain, sslPolicyErrors) => true;
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
            return await this.ExecuteRequestAsync(url: url, queryString: queryString, credentials: credentials, request: (client) => client.GetAsync(), token: token).ReceiveCompressedJsonAsync<T>();
        }

        public async Task GetAsync(string url, CancellationToken token, object queryString = null, RestCredentials credentials = null)
        {
            await this.ExecuteRequestAsync(url: url, queryString: queryString, credentials: credentials, request: (client) => client.GetAsync(), token: token);
        }

        public async Task<T> PostAsync<T>(string url, CancellationToken token, object request = null, RestCredentials credentials = null)
        {
            return await this.ExecuteRequestAsync(url: url, credentials: credentials, request: (client) => client.PostJsonAsync(request), token: token).ReceiveCompressedJsonAsync<T>();
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
