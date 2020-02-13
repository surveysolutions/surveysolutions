using System;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Refit;
using WB.Core.BoundedContexts.Headquarters.Designer;
using WB.Core.BoundedContexts.Headquarters.Implementation.Services;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.GenericSubdomains.Portable.Implementation;
using WB.Core.GenericSubdomains.Portable.Services;

namespace WB.UI.Headquarters.Services.Impl
{
    internal class DesignerApiFactory : IDesignerApiFactory
    {
        private readonly IRestServiceSettings serviceSettings;
        private readonly IDesignerUserCredentials designerUserCredentials;

        public DesignerApiFactory(
            IRestServiceSettings serviceSettings,
            IDesignerUserCredentials designerUserCredentials)
        {
            this.serviceSettings = serviceSettings;
            this.designerUserCredentials = designerUserCredentials;
        }

        public IDesignerApi Get()
        {
            return Get(this.designerUserCredentials);
        }      

        public IDesignerApi Get(IDesignerUserCredentials userCredentials)
        {
            var restHandler = new RestServiceHandler(userCredentials);
            var hc = new HttpClient(restHandler)
            {
                BaseAddress = new Uri(serviceSettings.Endpoint),
                Timeout = TimeSpan.FromMinutes(2),
                DefaultRequestHeaders =
                {
                    { "User-Agent",  serviceSettings.UserAgent },
                }
            };

            return RestService.For<IDesignerApi>(hc, new RefitSettings
            {
                ContentSerializer = new DesignerContentSerializer()
            });
        }

        /// <summary>
        /// JsonContentSerializer with ability to handle RestFile result separatly
        /// </summary>
        internal class DesignerContentSerializer : IContentSerializer
        {
            JsonContentSerializer json = new JsonContentSerializer();

            public async Task<T> DeserializeAsync<T>(HttpContent content)
            {
                if (typeof(T) == typeof(RestFile))
                {
                    object result = await AsRestFileAsync(content);
                    return (T)result;
                }

                return await json.DeserializeAsync<T>(content);
            }

            public Task<HttpContent> SerializeAsync<T>(T item)
            {
                return json.SerializeAsync(item);
            }

            public async Task<RestFile> AsRestFileAsync(HttpContent content)
            {
                var rawContentType = content?.Headers?.ContentType?.MediaType;
                var length = content?.Headers?.ContentLength;
                var fileName = content?.Headers?.ContentDisposition?.FileName;
                var fileContent = await content.ReadAsByteArrayAsync();

                return new RestFile(content: fileContent, contentType: rawContentType,
                   null, contentLength: length, fileName: fileName, HttpStatusCode.OK);
            }
        }

        // Handling Designer Errors as they were handled by RestService.
        internal class RestServiceHandler : HttpClientHandler
        {
            private readonly IDesignerUserCredentials designerCredentials;

            public RestServiceHandler(IDesignerUserCredentials designerCredentials)
            {
                this.designerCredentials = designerCredentials;
            }

            protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
            {
                try
                {
                    var credentials = designerCredentials.Get();
                    if (credentials != null)
                    {
                        request.Headers.Authorization = credentials.GetAuthenticationHeaderValue();
                    }
                    var call = new HttpCall(request);

                    call.Response = await base.SendAsync(request, cancellationToken);
                    call.Response.RequestMessage = request;
                    if (call.IsSucceeded)
                        return call.Response;

                    throw new ExtendedMessageHandlerException(call, null);
                }
                catch (OperationCanceledException ex)
                {
                    // throwed when receiving bytes in ReceiveBytesWithProgressAsync method and user canceling request
                    throw new RestException("Request canceled by user", type: RestExceptionType.RequestCanceledByUser, innerException: ex);
                }
                catch (ExtendedMessageHandlerException ex)
                {
                    if (ex.GetSelfOrInnerAs<TaskCanceledException>() != null || ex.GetSelfOrInnerAs<OperationCanceledException>() != null)
                    {
                        if (cancellationToken.IsCancellationRequested)
                        {
                            throw new RestException("Request timeout", type: RestExceptionType.RequestByTimeout,
                                statusCode: HttpStatusCode.RequestTimeout, innerException: ex);
                        }
                    }
                    else if (ex.Call.Response != null)
                    {
                        var reasonPhrase = await GetReasonPhrase(ex);
                        throw new RestException(reasonPhrase, statusCode: ex.Call.Response.StatusCode, innerException: ex);
                    }
                    else
                    {
                        var javaNetConnectionException = ex.GetSelfOrInner(e => e?.GetType().FullName == "Java.Net.ConnectException");
                        if (javaNetConnectionException != null)
                        {
                            throw new RestException(message: javaNetConnectionException.Message,
                                innerException: javaNetConnectionException,
                                type: RestExceptionType.HostUnreachable);
                        }

                        var sslHandshakeException = ex.GetSelfOrInner(e => e?.GetType().Name == "SSLHandshakeException");
                        if (sslHandshakeException != null)
                        {
                            throw new RestException(message: sslHandshakeException.Message,
                                innerException: sslHandshakeException,
                                type: RestExceptionType.UnacceptableCertificate);
                        }
                    }

                    throw new RestException(message: "Unexpected web exception", innerException: ex);
                }
            }

            private async Task<string> GetReasonPhrase(ExtendedMessageHandlerException ex)
            {
                try
                {
                    var responseMessage = ex.Call.Response;
                    var responseContent = await responseMessage.Content.ReadAsStringAsync();

                    var jsonFromHttpResponseMessage = JsonConvert.DeserializeObject<ResponseWithErrorMessage>(responseContent);
                    if (jsonFromHttpResponseMessage != null)
                        return jsonFromHttpResponseMessage.Message;
                }
                catch { } // if cant get message from response

                return ex.Call.Response.ReasonPhrase;
            }

            private class ResponseWithErrorMessage
            {
                public string Message { get; set; }
            }
        }
    }

}
