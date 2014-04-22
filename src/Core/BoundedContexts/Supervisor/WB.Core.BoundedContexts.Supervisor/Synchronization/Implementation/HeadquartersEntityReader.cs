using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using WB.Core.SharedKernel.Utils.Serialization;

namespace WB.Core.BoundedContexts.Supervisor.Synchronization.Implementation
{
    internal class HeadquartersEntityReader
    {
        private readonly IJsonUtils jsonUtils;
        private readonly HeadquartersSettings headquartersSettings;


        public HeadquartersEntityReader(IJsonUtils jsonUtils, HeadquartersSettings headquartersSettings)
        {
            this.jsonUtils = jsonUtils;
            this.headquartersSettings = headquartersSettings;
        }

        protected async Task<TEntity> GetEntityByUri<TEntity>(Uri uri)
        {
            using (var httpClient = new HttpClient())
            {
                if (!string.IsNullOrWhiteSpace(headquartersSettings.AccessToken))
                    httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(headquartersSettings.AccessToken);

                var httpRequestMessage = new HttpRequestMessage(HttpMethod.Get, uri);
                httpRequestMessage.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                HttpResponseMessage response = await httpClient.SendAsync(httpRequestMessage).ConfigureAwait(false);
                string resultString = await response.Content.ReadAsStringAsync().ConfigureAwait(false);

                var deserializedEntity = this.jsonUtils.Deserrialize<TEntity>(resultString);

                return deserializedEntity;
            }
        }
    }
}