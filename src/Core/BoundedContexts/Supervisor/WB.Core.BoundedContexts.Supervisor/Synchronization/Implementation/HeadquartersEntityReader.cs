using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using WB.Core.BoundedContexts.Supervisor.Extensions;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.SharedKernels.SurveySolutions.Services;

namespace WB.Core.BoundedContexts.Supervisor.Synchronization.Implementation
{
    internal class HeadquartersEntityReader
    {
        private readonly IJsonUtils jsonUtils;
        protected readonly IHeadquartersSettings headquartersSettings;
        private readonly Func<HttpMessageHandler> httpMessageHandler;

        public HeadquartersEntityReader(IJsonUtils jsonUtils, IHeadquartersSettings headquartersSettings, Func<HttpMessageHandler> messageHandler)
        {
            if (messageHandler == null) throw new ArgumentNullException("messageHandler");

            this.jsonUtils = jsonUtils;
            this.headquartersSettings = headquartersSettings;
            this.httpMessageHandler = messageHandler;
        }

        protected async Task<TEntity> GetEntityByUri<TEntity>(Uri uri)
        {
            using (var httpClient = new HttpClient(this.httpMessageHandler()))
            {
                httpClient.AppendAuthToken(this.headquartersSettings);

                var httpRequestMessage = new HttpRequestMessage(HttpMethod.Get, uri);
                httpRequestMessage.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                HttpResponseMessage response = await httpClient.SendAsync(httpRequestMessage).ConfigureAwait(false);
                string resultString = await response.Content.ReadAsStringAsync().ConfigureAwait(false);

                var deserializedEntity = this.jsonUtils.Deserialize<TEntity>(resultString);

                return deserializedEntity;
            }
        }
    }
}