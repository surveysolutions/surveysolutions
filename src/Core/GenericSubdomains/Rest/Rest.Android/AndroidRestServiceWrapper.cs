using System;
using System.Collections.Generic;
using System.Net;
using System.Security.Authentication;
using System.Threading;
using Microsoft.Practices.ServiceLocation;
using RestSharp;
using WB.Core.GenericSubdomains.Rest;
using WB.Core.GenericSubdomains.Logging;
using WB.Core.SharedKernel.Utils.Serialization;

namespace WB.Core.GenericSubdomains.Rest.Android
{
    internal class AndroidRestServiceWrapper : IRestServiceWrapper
    {
        private readonly string baseAddress;
        private readonly IJsonUtils jsonUtils;
        private readonly ILogger logger;
        private readonly bool acceptUnsignedCertificate;

        public AndroidRestServiceWrapper(string baseAddress, IJsonUtils jsonUtils, bool acceptUnsignedCertificate)
        {
            this.baseAddress = baseAddress;
            this.jsonUtils = jsonUtils;
            this.acceptUnsignedCertificate = acceptUnsignedCertificate;
            this.logger = ServiceLocator.Current.GetInstance<ILogger>();
        }

        public void ExecuteRestRequest(string url,  string login, string password, string method, params KeyValuePair<string, string>[] additionalParams)
        {
            var restClient = this.BuildRestClient(login, password);

            var request = this.BuildRequest(url, additionalParams, null, this.GetRequestMethod(method));

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

        public T ExecuteRestRequest<T>(string url, string login, string password, string method, params KeyValuePair<string, string>[] additionalParams)
        {
            var restClient = this.BuildRestClient(login, password);

            var request = this.BuildRequest(url, additionalParams, null, this.GetRequestMethod(method));

            var response = restClient.Execute(request);

            return this.HandlerResponse<T>(response);
        }

        public T ExecuteRestRequestAsync<T>(string url, CancellationToken ct, byte[] file, string fileName, string login, string password,
            string method,
            params KeyValuePair<string, string>[] additionalParams)
        {
            var restClient = this.BuildRestClient(login, password);

            var request = this.BuildRequest(url, additionalParams, file, fileName, this.GetRequestMethod(method));

            IRestResponse response = null;

            var token = restClient.ExecuteAsync(request, (r) => { response = r; });

            while (response == null)
            {
                if (ct.IsCancellationRequested)
                {
                    token.Abort();
                    throw new RestException("Operation was canceled.");
                }
            }

            return this.HandlerResponse<T>(response);
        }

        public T ExecuteRestRequestAsync<T>(string url, CancellationToken ct, object requestBody, string login, string password, string method,
            params KeyValuePair<string, string>[] additionalParams)
        {
            var restClient = this.BuildRestClient(login,password);

            var request = this.BuildRequest(url, additionalParams, requestBody, this.GetRequestMethod(method));
            
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

        private RestClient BuildRestClient(string login, string password)
        {
            var restClient = new RestClient(this.baseAddress);

            if (acceptUnsignedCertificate)
            {
                ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };
            }

            if (!string.IsNullOrEmpty(login) || !string.IsNullOrEmpty(password))
                restClient.Authenticator = new HttpBasicAuthenticator(login, password);
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
                var exception = new Exception(string.Format("Target returned unsupported result with status {0},{1}", response.StatusCode, response.StatusDescription));

                if (response.StatusCode == HttpStatusCode.Forbidden)
                    exception = new AuthenticationException("User is not authorized.");

                this.logger.Error("Sync error. Response status:" + response.StatusCode, exception);

                throw exception;
            }

            var syncItemsMetaContainer = this.jsonUtils.Deserrialize<T>(response.Content);

            if (syncItemsMetaContainer == null)
            {
                throw new Exception("Elements to be synchronized are not found.");
            }

            return syncItemsMetaContainer;
        }

        private RestRequest BuildRequest(string url, KeyValuePair<string, string>[] additionalParams, object requestBody, RestSharp.Method method)
        {
            var request = new RestRequest(url, method);
            request.RequestFormat = DataFormat.Json;

            request.AddHeader("Accept-Encoding", "gzip,deflate");

            if (requestBody != null && !string.IsNullOrWhiteSpace(requestBody.ToString()))
            {
                //request.AddBody(requestBody);
                request.AddParameter("application/json", requestBody, ParameterType.RequestBody);
            }

            foreach (var additionalParam in additionalParams)
            {
                request.AddParameter(additionalParam.Key, additionalParam.Value);
            }

            return request;
        }
        private RestRequest BuildRequest(string url, KeyValuePair<string, string>[] additionalParams, byte[] file,string fileName, RestSharp.Method method)
        {
            var request = new RestRequest(url, method);
            request.RequestFormat = DataFormat.Json;

            request.AddHeader("Accept-Encoding", "gzip,deflate");

            if (file != null && file.Length>0)
            {
                request.AddFile(fileName, file, fileName);
            }

            foreach (var additionalParam in additionalParams)
            {
                request.AddParameter(additionalParam.Key, additionalParam.Value);
            }

            return request;
        }
    }
}