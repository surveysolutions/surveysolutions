using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Security.Authentication;
using System.Threading;
using Microsoft.Practices.ServiceLocation;
using RestSharp;
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

        public void ExecuteRestRequest(string url,  string login, string password, string method, params KeyValuePair<string, object>[] additionalParams)
        {
            RestClient restClient = this.BuildRestClient(login, password);
            var request = this.BuildRequest(url, additionalParams, null, this.GetRequestMethod(method));
            var response = restClient.Execute(request);

            if (response.StatusCode == HttpStatusCode.OK) 
                return;

            if (response.StatusCode == HttpStatusCode.Forbidden || response.StatusCode == HttpStatusCode.Unauthorized)
                throw new AuthenticationException("Not authorized");

            this.logger.Error(string.Format("Sync error. Status: {0}, Response Uri: {1}, Url: {2} Method:{3}, Login: {4}, args: {5}",
                response.StatusDescription, response.ResponseUri, url, method, login, 
                string.Join(";", additionalParams.Select(x => x.Key + "=" + x.Value.ToString()).ToArray())));

            var exceptionMessage = string.IsNullOrWhiteSpace(response.Content)
                    ? string.Format("Target returned unexpected result. Status: {0}", response.StatusDescription)
                    : this.jsonUtils.Deserrialize<ErrorMessage>(response.Content).Message;

            throw new RestException(exceptionMessage);            
        }

        public T ExecuteRestRequest<T>(string url, string login, string password, string method, params KeyValuePair<string, object>[] additionalParams)
        {
            var restClient = this.BuildRestClient(login, password);
            var request = this.BuildRequest(url, additionalParams, null, this.GetRequestMethod(method));
            var response = restClient.Execute(request);

            return this.HandlerResponse<T>(response);
        }

        public T ExecuteRestRequestAsync<T>(string url, KeyValuePair<string, object>[] queryStringParams, CancellationToken ct, byte[] file, string fileName, string login, string password,
            string method, params KeyValuePair<string, object>[] additionalParams)
        {
            var restClient = this.BuildRestClient(login, password);
            var request = this.BuildRequest(url, queryStringParams, additionalParams, file, fileName, this.GetRequestMethod(method));
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

        public void ExecuteRestRequestAsync(string url, CancellationToken ct, byte[] file, string fileName, string login, string password,
           string method, params KeyValuePair<string, object>[] additionalParams)
        {
            var restClient = this.BuildRestClient(login, password);

            var request = this.BuildRequest(url, new KeyValuePair<string, object>[]{}, additionalParams, file, fileName, this.GetRequestMethod(method));

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

            if (response.StatusCode == HttpStatusCode.OK) 
                return;

            if (response.StatusCode == HttpStatusCode.Forbidden || response.StatusCode == HttpStatusCode.Unauthorized)
                throw new AuthenticationException("Not authorized");

            this.logger.Error(string.Format("Sync error. Response status: {0}. {1}", response.StatusCode, response.StatusDescription));
        
            var exceptionMessage = string.IsNullOrWhiteSpace(response.Content)
                    ? string.Format("Target returned unexpected result. Status: {0}", response.StatusDescription)
                    : this.jsonUtils.Deserrialize<ErrorMessage>(response.Content).Message;

            throw new RestException(exceptionMessage);
        }

        public T ExecuteRestRequestAsync<T>(string url, CancellationToken ct, object requestBody, string login, string password, string method,
            params KeyValuePair<string, object>[] additionalParams)
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
                this.logger.Error("Error occured during synchronization. Response contains exception. Message: " + response.ErrorMessage, response.ErrorException);
            }

            if (response.StatusCode != HttpStatusCode.OK)
            {
                this.logger.Error(
                    string.Format("Error at responce handling. Status: {0}, StatusCode: {1}, Content: '{2}'. {3}",
                        response.StatusDescription, response.StatusCode, response.Content, response.ErrorMessage), response.ErrorException);
                
                if (response.ErrorException != null)
                    throw response.ErrorException;

                string exceptionMessage;
                if (string.IsNullOrWhiteSpace(response.Content))
                {
                    exceptionMessage = GetErrorMessageFromResponce(response);
                }
                else
                {
                    try
                    {
                        exceptionMessage = this.jsonUtils.Deserrialize<ErrorMessage>(response.Content).Message;
                    }
                    catch (Exception e)
                    {
                        this.logger.Error(e.Message, e);
                        exceptionMessage = GetErrorMessageFromResponce(response);
                    }
                }
                throw new RestException(exceptionMessage, (int)response.StatusCode);
            }

            if (string.IsNullOrWhiteSpace(response.Content))
            {
                var message = string.Format("Empty content was returned, but expected content of type '{0}'", typeof (T).Name);
                this.logger.Error(message);
                throw new RestException(message);
            }
            try
            {
                var syncItemsMetaContainer = this.jsonUtils.Deserrialize<T>(response.Content);

                return syncItemsMetaContainer;
            }
            catch (Exception e)
            {
                this.logger.Error(string.Format("Returned content '{0}' can't be deserrialized with Type '{1}'", response.Content,
                    typeof(T).Name), e);
                throw new RestException(string.Format("Returned content can't be deserrialized with Type '{0}'", 
                    typeof(T).Name), e);
            }
        }

        private string GetErrorMessageFromResponce(IRestResponse response)
        {
            if (!string.IsNullOrWhiteSpace(response.ErrorMessage))
                return response.ErrorMessage;

            return string.Format("Status: '{0}'", response.StatusDescription);
        }

        private RestRequest BuildRequest(string url, IEnumerable<KeyValuePair<string, object>> additionalParams, object requestBody, RestSharp.Method method)
        {
            var request = new RestRequest(url, method)
            {
                RequestFormat = DataFormat.Json
            };
            request.AddHeader("Accept-Encoding", "gzip,deflate");

            if (requestBody != null && !string.IsNullOrWhiteSpace(requestBody.ToString()))
            {
                request.AddParameter("application/json", requestBody, ParameterType.RequestBody);
            }

            foreach (var additionalParam in additionalParams)
            {
                request.AddParameter(additionalParam.Key, additionalParam.Value);
            }

            return request;
        }
        private RestRequest BuildRequest(string url, IEnumerable<KeyValuePair<string, object>> queryStringParams, 
            IEnumerable<KeyValuePair<string, object>> additionalParams, byte[] file, string fileName, RestSharp.Method method)
        {
            var request = new RestRequest(url, method)
            {
                RequestFormat = DataFormat.Json
            };
            request.AddHeader("Accept-Encoding", "gzip,deflate");

            if (file != null && file.Length>0)
            {
                request.AddFile(fileName, file, fileName);
            }

            foreach (var additionalParam in queryStringParams)
            {
                request.AddParameter(additionalParam.Key, additionalParam.Value, ParameterType.UrlSegment);
            }
            
            foreach (var additionalParam in additionalParams)
            {
                request.AddParameter(additionalParam.Key, additionalParam.Value);
            }

            return request;
        }
    }
}