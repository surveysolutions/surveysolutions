using System;
using System.Net;
using System.Net.Http;
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
        private readonly IJsonUtils jsonUtils;

        public RestService(IRestServiceSettings restServiceSettings, ILogger logger, INetworkService networkService, IJsonUtils jsonUtils)
        {
            this.restServiceSettings = restServiceSettings;
            this.logger = logger;
            this.networkService = networkService;
            this.jsonUtils = jsonUtils;
        }

        private async Task<HttpResponseMessage> ExecuteRequestAsync(string url, Func<FlurlClient, Task<HttpResponseMessage>> request,
            object queryString = null, RestCredentials credentials = null)
        {
            if (this.networkService != null && !this.networkService.IsNetworkEnabled())
            {
                throw new RestException(Resources.NoNetwork);
            }

            string baseAddress = this.restServiceSettings.BaseAddress();
            if (string.IsNullOrWhiteSpace(baseAddress))
            {
                throw new ArgumentNullException("baseAddress", Resources.BaseAddressNotSet);
            }

            Url fullUrl = baseAddress
                .AppendPathSegment(url)
                .SetQueryParams(queryString);
            var restClient = fullUrl
                .WithHeader("Accept-Encoding", "gzip,deflate");

            if (credentials != null)
                restClient.WithBasicAuth(credentials.Login, credentials.Password);

            try
            {
                return await request(restClient);
            }
            catch (FlurlHttpException ex)
            {
                this.logger.Error(string.Format("Request to '{0}'. QueryParams: {1} failed. ", fullUrl, fullUrl.QueryParams), ex);

                if (ex.Call.Response != null)
                { 
                    var response = ex.Call.Response;
                    var message = response.Content.ReadAsStringAsync().Result;
                    var errorDescription =
                        this.TryParseRestErrorDescription(string.IsNullOrEmpty(message)
                            ? response.ReasonPhrase
                            : message);
                    throw new RestException(errorDescription.Message, statusCode: response.StatusCode, internalCode: errorDescription.Code, innerException: ex);
                }

                throw new RestException(message: Resources.NoConnection, innerException: ex);
            }
        }

        private RestErrorDescription TryParseRestErrorDescription(string response)
        {
            try
            {
                return jsonUtils.Deserialize<RestErrorDescription>(response);
            }
            catch (Exception)
            {
                return new RestErrorDescription
                       {
                           Message = response,
                           Code = SyncStatusCode.General
                       };
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
