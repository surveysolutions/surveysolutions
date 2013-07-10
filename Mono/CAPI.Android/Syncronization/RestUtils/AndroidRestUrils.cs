using System;
using System.Collections.Generic;
using System.Net;
using System.Security.Authentication;
using System.Threading;
using CAPI.Android.Core.Model.ModelUtils;
using Microsoft.Practices.ServiceLocation;
using RestSharp;
using WB.Core.GenericSubdomains.Logging;

namespace CAPI.Android.Syncronization.RestUtils
{
    public class AndroidRestUrils:IRestUrils
    {
        private readonly string baseAddress;

        private readonly ILogger logger;

        public AndroidRestUrils(string baseAddress)
        {
            this.baseAddress = baseAddress;
            this.logger = ServiceLocator.Current.GetInstance<ILogger>();
        }

        public void ExcecuteRestRequest(string url, params KeyValuePair<string, string>[] additionalParams)
        {
            var restClient = new RestClient(this.baseAddress);

            var request = BuildRequest(url, additionalParams);

            var response = restClient.Execute(request);

            if (response.StatusCode != HttpStatusCode.OK)
            {
                var exception = new Exception("Target returned unsupported result.");

                if (response.StatusCode == HttpStatusCode.Forbidden)
                    exception = new AuthenticationException("user wasn't authorized");

                var logger = ServiceLocator.Current.GetInstance<ILogger>();

                logger.Error("Sync error. Responce status:" + response.StatusCode, exception);

                throw exception;
            }
        }

        public T ExcecuteRestRequest<T>(string url, params KeyValuePair<string, string>[] additionalParams)
        {
            var restClient = new RestClient(this.baseAddress);

            var request = BuildRequest(url, additionalParams);

            var response = restClient.Execute(request);

            return HandlerResponce<T>(response);
        }

        public T ExcecuteRestRequestAsync<T>(string url, CancellationToken ct, params KeyValuePair<string, string>[] additionalParams)
        {
            var restClient = new RestClient(this.baseAddress);

            var request = BuildRequest(url, additionalParams);

            IRestResponse response = null;

            var token = restClient.ExecuteAsync(request, (r) => { response = r; });

            while (response==null)
            {
                if (ct.IsCancellationRequested)
                {
                    token.Abort();
                    throw new RestException("Operation was canceled.");
                }
                Thread.Sleep(500);
            }

            return HandlerResponce<T>(response);
        }

        private T HandlerResponce<T>(IRestResponse response)
        {
            if (response.ErrorException != null)
            {
                this.logger
                          .Error("Sync error, response contains exception. Message:" + response.ErrorMessage, 
                                 response.ErrorException);
                throw new Exception("Error occured on communication with target. Please, check settings.");
            }

            if (string.IsNullOrWhiteSpace(response.Content) || response.StatusCode != HttpStatusCode.OK)
            {
                var exception = new Exception("Target returned unsupported result.");

                if (response.StatusCode == HttpStatusCode.Forbidden)
                    exception = new AuthenticationException("User is not authorized.");

                this.logger
                          .Error("Sync error. Responce status:" + response.StatusCode, exception);

                throw exception;
            }

            var syncItemsMetaContainer = JsonUtils.GetObject<T>(response.Content);

            if (syncItemsMetaContainer == null)
            {
                throw new Exception("Elements to be synchronized are not found.");
            }

            return syncItemsMetaContainer;
        }
       
        private RestRequest BuildRequest(string url, KeyValuePair<string, string>[] additionalParams)
        {
            var request = new RestRequest(url, Method.POST);
            request.RequestFormat = DataFormat.Json;

            request.AddHeader("Accept-Encoding", "gzip,deflate");
            foreach (var additionalParam in additionalParams)
            {
                request.AddParameter(additionalParam.Key, additionalParam.Value);
            }
            return request;
        }
    }
}