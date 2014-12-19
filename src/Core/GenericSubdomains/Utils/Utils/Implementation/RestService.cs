using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using RestSharp.Portable;
using RestSharp.Portable.Authenticators;
using RestSharp.Portable.Deserializers;
using RestSharp.Portable.Encodings;
using RestSharp.Portable.Serializers;
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
        private readonly ISerializer jsonSerializer;
        private readonly IDeserializer jsonDeserializer;


        public RestService(IRestServiceSettings restServiceSettings, ILogger logger, INetworkService networkService, IJsonUtils jsonUtils, ILocalizationService localizationService)
        {
            this.restServiceSettings = restServiceSettings;
            this.logger = logger;
            this.networkService = networkService;
            this.localizationService = localizationService;
            this.jsonSerializer = new RestSerializer(jsonUtils);
            this.jsonDeserializer = new RestDeserializer(jsonUtils);
        }

        private async Task SendRequestAsync(string url, HttpMethod verb, dynamic request = null, IEnumerable<RestAttachment> attachments = null,
            RestCredentials credentials = null, CancellationToken token = default(CancellationToken))
        {
            await this.SendRequestAsync<string>(url: url, verb: verb, request: request, attachments: attachments, credentials: credentials, token: token);
        }

        private async Task<T> SendRequestAsync<T>(string url, HttpMethod verb, object request = null, IEnumerable<RestAttachment> attachments = null,
            RestCredentials credentials = null, CancellationToken token = default(CancellationToken))
        {
            var client = CreateRestClient(credentials);
            var clientRequest = CreateRestRequest(url: url, verb: verb, request: request, attachments: attachments);
            
            try
            {
                var response = await client.Execute<T>(clientRequest, token);

                if (response.StatusCode == HttpStatusCode.NoContent)
                    return default(T);

                if (response.StatusCode == HttpStatusCode.OK)
                    return response.Data;

                throw new RestException(response.StatusDescription, (int) response.StatusCode);
            }
            catch (HttpRequestException ex)
            {
                this.logger.Error(
                    string.Format("REST Service: Request exception when connect to {0}/{1}, method {2}",
                        this.restServiceSettings.BaseAddress(), clientRequest.Resource, clientRequest.Method), ex);

                throw new RestException(message: this.localizationService.GetString("NoConnection"), innerException: ex);
            }
            catch (WebException ex)
            {
                throw new RestException(message: this.localizationService.GetString("NoConnection"), innerException: ex);
            }
            catch (Exception ex)
            {
                this.logger.Error(
                    string.Format("REST Service: Unhandled exception when connect to {0}/{1}, method {2}",
                        this.restServiceSettings.BaseAddress(), clientRequest.Resource, clientRequest.Method), ex);

                throw;
            }
        }

        private RestRequest CreateRestRequest(string url, HttpMethod verb, object request = null, IEnumerable<RestAttachment> attachments = null)
        {
            var clientRequest = new RestRequest(url, verb)
            {
                Serializer = this.jsonSerializer
            };

            clientRequest.AddHeader("Accept-Encoding", "gzip,deflate");

            var defaultParameters = new List<Parameter>(clientRequest.Parameters);

            if (request != null) clientRequest.AddObject(request);
            if (attachments != null)
            {
                foreach (var requestParametr in clientRequest.Parameters.Except(defaultParameters))
                {
                    requestParametr.Type = ParameterType.QueryString;
                }
                foreach (var attachment in attachments)
                {
                    clientRequest.AddFile(attachment.AttachmentName, attachment.Data, attachment.FileName);
                }
            }

            return clientRequest;
        }

        private RestClient CreateRestClient(RestCredentials credentials)
        {
            if (this.networkService != null && !this.networkService.IsNetworkEnabled())
            {
                throw new RestException(this.localizationService.GetString("NoNetwork"));
            }

            var client = new RestClient(this.restServiceSettings.BaseAddress());

            client.AddHandler(this.jsonSerializer.ContentType.MediaType, this.jsonDeserializer);
            client.AddEncoding("gzip", new GzipEncoding());
            client.AddEncoding("deflate", new DeflateEncoding());

            if (credentials != null)
                client.Authenticator = new HttpBasicAuthenticator(credentials.Login, credentials.Password);
            return client;
        }

        public async Task<T> GetAsync<T>(string url, dynamic request = null, RestCredentials credentials = null, CancellationToken token = default(CancellationToken)) 
        {
            return await SendRequestAsync<T>(url: url, verb: HttpMethod.Get, request: request, credentials: credentials, token: token);
        }

        public async Task GetAsync(string url, dynamic request = null, RestCredentials credentials = null, CancellationToken token = new CancellationToken())
        {
            await SendRequestAsync(url: url, verb: HttpMethod.Get, request: request, credentials: credentials, token: token);
        }

        public async Task<T> PostAsync<T>(string url, dynamic request = null, RestCredentials credentials = null, CancellationToken token = default(CancellationToken))
        {
            return await SendRequestAsync<T>(url: url, verb: HttpMethod.Post, request: request, credentials: credentials, token: token);
        }

        public async Task PostAsync(string url, dynamic request = null, RestCredentials credentials = null, CancellationToken token = new CancellationToken())
        {
            await SendRequestAsync(url: url, verb: HttpMethod.Post, request: request, credentials: credentials, token: token);
        }

        public async Task PostWithAttachmentsAsync(string url, IEnumerable<RestAttachment> attachments = null, dynamic request = null,
            RestCredentials credentials = null, CancellationToken token = new CancellationToken())
        {
            await SendRequestAsync(url: url, verb: HttpMethod.Post, request: request, attachments: attachments, credentials: credentials, token: token);
        }
    }
}
