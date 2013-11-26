using System;
using System.Collections.Generic;
using System.Net;
using System.Security.Authentication;
using System.Threading;
using Java.Security.Cert;
using Microsoft.Practices.ServiceLocation;
using RestSharp;
using WB.Core.BoundedContexts.Capi.ModelUtils;
using WB.Core.GenericSubdomains.Logging;

namespace WB.UI.Shared.Android.RestUtils
{
    public class AndroidRestUrils : IRestUrils
    {
        private readonly string baseAddress;

        private readonly ILogger logger;

        public AndroidRestUrils(string baseAddress)
        {
            this.baseAddress = baseAddress;
            this.logger = ServiceLocator.Current.GetInstance<ILogger>();
        }

        public void ExcecuteRestRequest(string url, IAuthenticator authenticator, string method, params KeyValuePair<string, string>[] additionalParams)
        {
            var restClient = BuildRestClient(authenticator);

            var request = BuildRequest(url, additionalParams, null, GetRequestMethod(method));

            var response = restClient.Execute(request);

            if (response.StatusCode != HttpStatusCode.OK)
            {
                var exception = new Exception("Target returned unsupported result.");

                if (response.StatusCode == HttpStatusCode.Forbidden)
                    exception = new AuthenticationException("user wasn't authorized");
                
                this.logger.Error("Sync error. Response status:" + response.StatusCode, exception);

                throw exception;
            }
        }

        public T ExcecuteRestRequest<T>(string url, IAuthenticator authenticator, string method, params KeyValuePair<string, string>[] additionalParams)
        {
            var restClient = BuildRestClient(authenticator);

            var request = BuildRequest(url, additionalParams, null, GetRequestMethod(method));

            var response = restClient.Execute(request);

            return this.HandlerResponse<T>(response);
        }

        public T ExcecuteRestRequestAsync<T>(string url, CancellationToken ct, string requestBody, IAuthenticator authenticator, string method,
            params KeyValuePair<string, string>[] additionalParams)
        {
            var restClient = BuildRestClient(authenticator);

            var request = BuildRequest(url, additionalParams, requestBody, GetRequestMethod(method));
            
            IRestResponse response = null;

            var token = restClient.ExecuteAsync(request, (r) => { response = r; });

            while (response==null)
            {
                if (ct.IsCancellationRequested)
                {
                    token.Abort();
                    throw new RestException("Operation was canceled.");
                }
            }

            return this.HandlerResponse<T>(response);
        }

        private RestClient BuildRestClient(IAuthenticator authenticator)
        {
            var restClient = new RestClient(this.baseAddress);

            ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };

            if (authenticator != null)
                restClient.Authenticator = authenticator;
            return restClient;
        }

        private Method GetRequestMethod(string method)
        {
            Method result;
            if (string.IsNullOrEmpty(method) || !Method.TryParse(method, out result))
                return Method.POST;
            return result;
        }

        private T HandlerResponse<T>(IRestResponse response)
        {
            if (response.ErrorException != null)
            {
                this.logger
                          .Error("Sync error, response contains exception. Message:" + response.ErrorMessage, 
                                 response.ErrorException);
                throw new Exception("Error occurred on communication with target. Please, check settings or try again later.");
            }

            if (string.IsNullOrWhiteSpace(response.Content) || response.StatusCode != HttpStatusCode.OK)
            {
                var exception = new Exception("Target returned unsupported result.");

                if (response.StatusCode == HttpStatusCode.Forbidden)
                    exception = new AuthenticationException("User is not authorized.");

                this.logger.Error("Sync error. Response status:" + response.StatusCode, exception);

                throw exception;
            }

            var syncItemsMetaContainer = JsonUtils.GetObject<T>(response.Content);

            if (syncItemsMetaContainer == null)
            {
                throw new Exception("Elements to be synchronized are not found.");
            }

            return syncItemsMetaContainer;
        }

        private RestRequest BuildRequest(string url, KeyValuePair<string, string>[] additionalParams, string requestBody, RestSharp.Method method)
        {
            var request = new RestRequest(url, method);
            request.RequestFormat = DataFormat.Json;

            request.AddHeader("Accept-Encoding", "gzip,deflate");

            if (!string.IsNullOrWhiteSpace(requestBody))
            {
                //request.AddBody(requestBody);
                request.AddParameter("application/json", requestBody, ParameterType.RequestBody);
            }
            else
            {
                foreach (var additionalParam in additionalParams)
                {
                    request.AddParameter(additionalParam.Key, additionalParam.Value);
                }
            }

            return request;
        }
    }
}