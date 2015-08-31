using System;
using System.Net.Http;
using System.Threading.Tasks;

using Flurl.Http;

using WB.Core.GenericSubdomains.Portable.Rest;

namespace WB.Core.GenericSubdomains.Native.Rest
{
    public class FlurlRestClient : IRestClient
    {
        private readonly FlurlClient restClient;

        public FlurlRestClient(FlurlClient restClient)
        {
            this.restClient = restClient;
        }

        public async Task<HttpResponseMessage> PostJsonAsync(object request)
        {
            try
            {
                return await this.restClient.PostJsonAsync(request);
            }
            catch (FlurlHttpTimeoutException ex)
            {
                throw new RestHttpTimeoutException(ex.Message, ex);
            }
            catch (FlurlHttpException ex)
            {
                var restException = new RestHttpException(ex.Message, ex);
                if (ex.Call.Response != null)
                {
                    restException.ReasonPhrase = ex.Call.Response.ReasonPhrase;
                    restException.StatusCode = ex.Call.Response.StatusCode;
                }

                throw restException;
            }
        }

        public async Task<HttpResponseMessage> GetAsync()
        {
            try
            {
                return await this.restClient.GetAsync();
            }
            catch (FlurlHttpTimeoutException ex)
            {
                throw new RestHttpTimeoutException(ex.Message, ex);
            }
            catch (FlurlHttpException ex)
            {
                var restException = new RestHttpException(ex.Message, ex);
                if (ex.Call.Response != null)
                {
                    restException.ReasonPhrase = ex.Call.Response.ReasonPhrase;
                    restException.StatusCode = ex.Call.Response.StatusCode;
                }

                throw restException;
            }
          
        }

        public string GetFullUrl()
        {
            return this.restClient.Url.ToString();
        }
    }
}