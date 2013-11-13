using System;
using System.Collections.Generic;
using System.Net;
using System.Security.Authentication;
using System.Threading;
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

        public void ExcecuteRestRequest(string url, IAuthenticator authenticator, params KeyValuePair<string, string>[] additionalParams)
        {
            var restClient = new RestClient(this.baseAddress);

            var request = this.BuildRequest(url, additionalParams, null);

            if (authenticator != null)
                restClient.Authenticator = authenticator;

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

        public T ExcecuteRestRequest<T>(string url, IAuthenticator authenticator, params KeyValuePair<string, string>[] additionalParams)
        {
            var restClient = new RestClient(this.baseAddress);

            var request = this.BuildRequest(url, additionalParams, null);

            if (authenticator != null)
                restClient.Authenticator = authenticator;

            var response = restClient.Execute(request);

            return this.HandlerResponse<T>(response);
        }

        public T ExcecuteRestRequestAsync<T>(string url, CancellationToken ct, string requestBody, IAuthenticator authenticator,
            params KeyValuePair<string, string>[] additionalParams)
        {
            var restClient = new RestClient(this.baseAddress);

            var request = this.BuildRequest(url, additionalParams, requestBody);

            if (authenticator != null)
                restClient.Authenticator = authenticator;
            
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

        private RestRequest BuildRequest(string url, KeyValuePair<string, string>[] additionalParams, string requestBody)
        {
            var request = new RestRequest(url, Method.POST);
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