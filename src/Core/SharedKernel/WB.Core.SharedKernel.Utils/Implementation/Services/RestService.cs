using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using RestSharp;
using RestSharp.Deserializers;
using RestSharp.Serializers;
using WB.Core.GenericSubdomains.Logging;
using WB.Core.GenericSubdomains.Utils.Services;
using WB.Core.SharedKernel.Utils.Services;
using WB.Core.SharedKernel.Utils.Services.Rest;

namespace WB.Core.SharedKernel.Utils.Implementation.Services
{
    public class RestService : IRestService
    {
        private readonly IRestServiceSettings restServiceSettings;
        private readonly ILogger logger;
        private readonly INetworkService networkService;
        private readonly ISerializer jsonSerializer;
        private readonly IDeserializer jsonDeserializer;


        public RestService(IRestServiceSettings restServiceSettings, ILogger logger, INetworkService networkService, IJsonUtils jsonUtils)
        {
            this.restServiceSettings = restServiceSettings;
            this.logger = logger;
            this.networkService = networkService;
            this.jsonSerializer = new RestSerializer(jsonUtils);
            this.jsonDeserializer = new RestDeserializer(jsonUtils);
        }

        private Task SendRequest(string url, Method verb, dynamic requestBody = null,
            dynamic requestQueryString = null,
            RestCredentials credentials = null, CancellationToken token = default(CancellationToken))
        {
            return this.SendRequest<string>(url: url, verb: Method.POST, requestBody: requestBody, requestQueryString: requestQueryString, credentials: credentials, token: token);
        }

        private async Task<T> SendRequest<T>(string url, Method verb, object requestBody = null, object requestQueryString = null,
            RestCredentials credentials = null, CancellationToken token = default(CancellationToken))
        {
            var client = CreateRestClient(credentials);
            var request = CreateRestRequest(url: url, verb: verb, requestBody: requestBody,
                requestQueryString: requestQueryString);

            var response = await client.ExecuteTaskAsync<T>(request, token).ConfigureAwait(false);

            if (response.ResponseStatus == ResponseStatus.Error)
                throw new RestException(response.ErrorException.Message, (int) HttpStatusCode.ServiceUnavailable);

            if (response.ResponseStatus == ResponseStatus.Completed)
            {
                if (response.StatusCode == HttpStatusCode.NoContent)
                    return default(T);

                if (response.StatusCode == HttpStatusCode.OK)
                    return response.Data;


                throw new RestException(response.StatusDescription, (int) response.StatusCode);
            }


            const string unhandledExceptionMessage = "REST Service: Unhandled exception";
            this.logger.Error(unhandledExceptionMessage, response.ErrorException);
            throw new RestException(unhandledExceptionMessage, (int) HttpStatusCode.ServiceUnavailable);
        }

        private RestRequest CreateRestRequest(string url, Method verb, dynamic requestBody = null, dynamic requestQueryString = null)
        {
            var request = new RestRequest(url, verb)
            {
                RequestFormat = DataFormat.Json,
                JsonSerializer = this.jsonSerializer
            };

            request.AddHeader("Accept-Encoding", "gzip,deflate");

            if (requestQueryString != null) request.AddObject(requestQueryString);
            if (requestBody != null) request.AddBody(requestBody);

            return request;
        }

        private RestClient CreateRestClient(RestCredentials credentials)
        {
            if (this.networkService != null && !this.networkService.IsNetworkEnabled())
            {
                throw new RestException("Network is unavailable");
            }

            var client = new RestClient(this.restServiceSettings.BaseAddress());

            client.AddHandler(this.jsonSerializer.ContentType, this.jsonDeserializer);

            if (credentials != null)
                client.Authenticator = new HttpBasicAuthenticator(credentials.Login, credentials.Password);
            return client;
        }

        public Task<T> GetAsync<T>(string url, dynamic requestBody = null, dynamic requestQueryString = null, RestCredentials credentials = null,
            CancellationToken token = default(CancellationToken)) 
        {
            return SendRequest<T>(url: url, verb: Method.GET, requestBody: requestBody, requestQueryString: requestQueryString, credentials: credentials,
                token: token);
        }

        public Task GetAsync(string url, dynamic requestBody = null, dynamic requestQueryString = null, RestCredentials credentials = null,
            CancellationToken token = new CancellationToken())
        {
             return SendRequest(url: url, verb: Method.GET, requestBody: requestBody, requestQueryString: requestQueryString, credentials: credentials,
                token: token);
        }

        public Task<T> PostAsync<T>(string url, dynamic requestBody = null, dynamic requestQueryString = null, RestCredentials credentials = null,
            CancellationToken token = default(CancellationToken))
        {
            return SendRequest<T>(url: url, verb: Method.POST, requestBody: requestBody, requestQueryString: requestQueryString, credentials: credentials,
                token: token);
        }

        public Task PostAsync(string url, dynamic requestBody = null, dynamic requestQueryString = null, RestCredentials credentials = null,
            CancellationToken token = new CancellationToken())
        {
            return SendRequest(url: url, verb: Method.POST, requestBody: requestBody, requestQueryString: requestQueryString, credentials: credentials,
                token: token);
        }
    }
}
