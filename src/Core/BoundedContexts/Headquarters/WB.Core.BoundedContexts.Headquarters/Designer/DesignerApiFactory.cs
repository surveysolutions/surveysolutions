using Newtonsoft.Json;
using Refit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using WB.Core.BoundedContexts.Headquarters.Implementation.Services;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.GenericSubdomains.Portable.Implementation;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.SharedKernel.Structures.Synchronization.Designer;
using WB.Core.SharedKernels.Questionnaire.Translations;

namespace WB.Core.BoundedContexts.Headquarters.Designer
{
    public interface IDesignerApiFactory
    {
        IDesignerApi Get();
    }

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
            var hc = new HttpClient()
            {
                BaseAddress = new Uri(serviceSettings.Endpoint),
                DefaultRequestHeaders =
                {
                    { "User-Agent",  serviceSettings.UserAgent },
                }
            };

            var credentials = designerUserCredentials.Get();
            if (credentials != null)
            {
                hc.DefaultRequestHeaders.Authorization = credentials.GetAuthenticationHeaderValue();
            }

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
                if(typeof(T) == typeof(RestFile))
                {
                    object result = await AsRestFileAsync(content);
                    return (T) result;
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
            protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
            {
                try
                {
                    var result = await base.SendAsync(request, cancellationToken);
                    return result;
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
                        var reasonPhrase = GetReasonPhrase(ex);
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

            private string GetReasonPhrase(ExtendedMessageHandlerException ex)
            {
                try
                {
                    var responseMessage = ex.Call.Response;
                    var responseContent = responseMessage.Content.ReadAsStringAsync().Result;

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
