using System;
using System.Net;
using System.Net.Http;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using System.Threading.Tasks;
using Flurl;
using Flurl.Http;
using WB.Core.GenericSubdomains.Portable.Implementation;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure;
using WB.Core.GenericSubdomains.Portable.Services;
using ILogger = WB.Core.GenericSubdomains.Portable.Services.ILogger;

namespace WB.UI.Tester.Infrastructure.Internals.Rest
{
    internal class RestService : IRestService
    {
        private readonly ITesterSettings settings;
        private readonly ITesterNetworkService networkService;
        private static RemoteCertificateValidationCallback defautCallback = null;

        public RestService(ITesterSettings settings, ILogger logger, ITesterNetworkService networkService)
        {
            if (settings == null) throw new ArgumentNullException("settings");
            if (networkService == null) throw new ArgumentNullException("networkService");

            this.settings = settings;
            this.networkService = networkService;
        }

        private async Task<HttpResponseMessage> ExecuteRequestAsync(string url, Func<FlurlClient, Task<HttpResponseMessage>> request, CancellationToken token,
            object queryString = null, RestCredentials credentials = null)
        {
            if (!this.networkService.IsEndpointReachable())
            {
                throw new RestException("No connection to site endpoint");
            }
            this.AcceptUnsignedSslCertificate();

            var fullUrl = this.settings.Endpoint
                .AppendPathSegment(url)
                .SetQueryParams(queryString);

            var restClient = fullUrl
                .ConfigureHttpClient(http => new HttpClient(new RestMessageHandler(token)))
                .WithTimeout(this.settings.RequestTimeout)
                .WithHeader("Accept-Encoding", "gzip,deflate");

            if (credentials != null)
                restClient.WithBasicAuth(credentials.Login, credentials.Password);

            try
            {
                return await request(restClient);
            }
            catch (ArgumentException ex)
            {
                throw new RestException(
                    message: string.Format("Invalid endpoint url {0}", this.settings.Endpoint),
                    statusCode: HttpStatusCode.BadRequest,
                    innerException: ex);
            }
            catch (UriFormatException ex)
            {
                throw new RestException(
                    message: string.Format("Invalid endpoint url {0}", this.settings.Endpoint),
                    statusCode: HttpStatusCode.BadRequest,
                    innerException: ex);
            }
            catch (FlurlHttpTimeoutException ex)
            {
                throw new RestException(message: "Request timeout", statusCode: HttpStatusCode.RequestTimeout,
                    innerException: ex);
            }
            catch (OperationCanceledException ex)
            {
                throw new RestException(message: "Request timeout", statusCode: HttpStatusCode.RequestTimeout,
                    innerException: ex);
            }
            catch (FlurlHttpException ex)
            {
                if (ex.Call.Response != null)
                    throw new RestException(ex.Call.Response.ReasonPhrase, statusCode: ex.Call.Response.StatusCode,
                        innerException: ex);

                throw new RestException(message: "No connection", innerException: ex);
            }
            catch (Exception ex)
            {
                throw new RestException(message: "No connection", innerException: ex);
            }
        }

        private void AcceptUnsignedSslCertificate()
        {
            if (ServicePointManager.ServerCertificateValidationCallback != AcceptAnyCertificateValidationCallback)
            {
                defautCallback = ServicePointManager.ServerCertificateValidationCallback;
            }

            ServicePointManager.ServerCertificateValidationCallback =
                this.settings.AcceptUnsignedSslCertificate ? AcceptAnyCertificateValidationCallback : defautCallback;
        }

        private static bool AcceptAnyCertificateValidationCallback(object sender, X509Certificate cert, X509Chain chain, SslPolicyErrors sslPolicyErrors)
        {
            return true;
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
