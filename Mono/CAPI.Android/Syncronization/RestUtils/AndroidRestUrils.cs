using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Security.Authentication;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Newtonsoft.Json;
using RestSharp;
using WB.Core.SharedKernel.Utils.Logging;

namespace CAPI.Android.Syncronization.RestUtils
{
    public class AndroidRestUrils:IRestUrils
    {
        private readonly string baseAddress;

        public AndroidRestUrils(string baseAddress)
        {
            this.baseAddress = baseAddress;
        }

        public void ExcecuteRestRequest(string url, string bodyContent, params KeyValuePair<string, string>[] additionalParams)
        {
            var restClient = new RestClient(this.baseAddress);

            var request = new RestRequest(url, Method.POST);
            request.RequestFormat = DataFormat.Json;

            request.AddHeader("Accept-Encoding", "gzip,deflate");

            foreach (var additionalParam in additionalParams)
            {
                request.AddParameter(additionalParam.Key, additionalParam.Value);
            }

            request.AddBody(bodyContent);

            var response = restClient.Execute(request);
            if (response.StatusCode != HttpStatusCode.OK)
            {
                var exception = new Exception("Target returned unsupported result.");

                if (response.StatusCode == HttpStatusCode.Forbidden)
                    exception = new AuthenticationException("user wasn't authorized");

                LogManager.GetLogger(GetType())
                          .Error("Sync error. Responce status:" + response.StatusCode, exception);

                throw exception;
            }
        }

        public void ExcecuteRestRequest(string url, params KeyValuePair<string, string>[] additionalParams)
        {
            var restClient = new RestClient(this.baseAddress);

            var request = new RestRequest(url, Method.POST);
            request.RequestFormat = DataFormat.Json;

            request.AddHeader("Accept-Encoding", "gzip,deflate");
            foreach (var additionalParam in additionalParams)
            {
                request.AddParameter(additionalParam.Key, additionalParam.Value);
            }
            var response = restClient.Execute(request);

            if (response.StatusCode != HttpStatusCode.OK)
            {
                var exception = new Exception("Target returned unsupported result.");

                if (response.StatusCode == HttpStatusCode.Forbidden)
                    exception = new AuthenticationException("user wasn't authorized");

                LogManager.GetLogger(GetType())
                          .Error("Sync error. Responce status:" + response.StatusCode, exception);

                throw exception;
            }
        }

        public T ExcecuteRestRequest<T>(string url, params KeyValuePair<string, string>[] additionalParams) where T : class
        {
            var restClient = new RestClient(this.baseAddress);

            var request = new RestRequest(url, Method.POST);
            request.RequestFormat = DataFormat.Json;

            request.AddHeader("Accept-Encoding", "gzip,deflate");
            foreach (var additionalParam in additionalParams)
            {
                request.AddParameter(additionalParam.Key, additionalParam.Value);
            }
            var response = restClient.Execute(request);

            if (string.IsNullOrWhiteSpace(response.Content) || response.StatusCode != HttpStatusCode.OK)
            {
                var exception = new Exception("Target returned unsupported result.");

                if (response.StatusCode == HttpStatusCode.Forbidden)
                    exception = new AuthenticationException("user wasn't authorized");

                LogManager.GetLogger(GetType())
                          .Error("Sync error. Responce status:" + response.StatusCode, exception);

                throw exception;
            }

            var syncItemsMetaContainer =
                JsonConvert.DeserializeObject<T>(response.Content, new JsonSerializerSettings());

            if (syncItemsMetaContainer == null)
            {
                throw new Exception("Elements to be synchronized are not found.");
            }
            return syncItemsMetaContainer;
        }
    }
}