using System;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using RestSharp.Portable;
using RestSharp.Portable.Authenticators;
using RestSharp.Portable.Serializers;
using WB.Core.GenericSubdomains.Logging;
using WB.Core.GenericSubdomains.Utils.Services;
using WB.Core.GenericSubdomains.Utils.Services.Rest;

namespace WB.Core.GenericSubdomains.Utils.Implementation.Services.Rest
{
    public class RestService : IRestService
    {
        private readonly IRestServiceSettings restServiceSettings;
        private readonly ILogger logger;
        private readonly INetworkService networkService;
        private readonly ISerializer restSerializer;

        public RestService(IRestServiceSettings restServiceSettings, IJsonUtils jsonUtils, ILogger logger, INetworkService networkService)
        {
            this.restServiceSettings = restServiceSettings;
            this.logger = logger;
            this.networkService = networkService;
            this.restSerializer = new RestSerializer(jsonUtils);
        }

        private async Task SendRequest(string url, HttpMethod verb, dynamic requestData = null,
            RestCredentials credentials = null, CancellationToken token = default(CancellationToken))
        {
            await SendRequest<bool>(url: url, verb: verb, requestData: requestData, credentials: credentials, token: token);
        }

        private async Task<T> SendRequest<T>(string url, HttpMethod verb, dynamic requestData = null,
            RestCredentials credentials = null, CancellationToken token = default(CancellationToken))
        {
            if (!this.networkService.IsNetworkEnabled())
            {
                throw new RestException("Network is unavailable");
            }

            var client = CreateRestClient(credentials);
            var request = CreateRestRequest(url, verb, requestData);

            try
            {
                var response = await client.Execute<T>(request, token);
                if (response.StatusCode != HttpStatusCode.OK)
                {
                    throw new RestException(response.StatusDescription, (int)response.StatusCode);
                }

                return response.Data;
            }
            catch (Exception ex)
            {
                var restException = ex as RestException;
                if (restException != null)
                {
                    throw restException;
                }

                this.logger.Error("Rest unhandled exception", ex);
                throw new RestException("Unhandled exception", (int)HttpStatusCode.ServiceUnavailable);
            }
        }

        private RestRequest CreateRestRequest(string url, HttpMethod verb, object requestData)
        {
            var request = new RestRequest(url, verb) { Serializer = this.restSerializer };
            request.AddHeader("Accept-Encoding", "gzip,deflate");
            if (requestData != null)
                request.AddObject(requestData);

            return request;
        }

        private RestClient CreateRestClient(RestCredentials credentials)
        {
            var client = new RestClient(this.restServiceSettings.BaseAddress());
            if (credentials != null)
                client.Authenticator = new HttpBasicAuthenticator(credentials.Login, credentials.Password);
            return client;
        }

        public Task<T> GetAsync<T>(string url, dynamic requestData = null, RestCredentials credentials = null,
            CancellationToken token = default(CancellationToken))
        {
            return SendRequest<T>(url: url, verb: HttpMethod.Get, requestData: requestData, credentials: credentials,
                token: token);
        }

        public Task GetAsync(string url, dynamic requestData = null, RestCredentials credentials = null,
            CancellationToken token = new CancellationToken())
        {
            return SendRequest(url: url, verb: HttpMethod.Get, requestData: requestData, credentials: credentials,
                token: token);
        }

        public Task<T> PostAsync<T>(string url, dynamic requestData = null, RestCredentials credentials = null,
            CancellationToken token = default(CancellationToken))
        {
            return SendRequest<T>(url: url, verb: HttpMethod.Post, requestData: requestData, credentials: credentials,
                token: token);
        }

        public Task PostAsync(string url, dynamic requestData = null, RestCredentials credentials = null,
            CancellationToken token = new CancellationToken())
        {
            return SendRequest(url: url, verb: HttpMethod.Post, requestData: requestData, credentials: credentials,
                token: token);
        }
    }
}
