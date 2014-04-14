using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Main.Core.Documents;
using Newtonsoft.Json;

namespace WB.Core.BoundedContexts.Supervisor.Users.Implementation
{
    internal class HeadquartersUserReader : IHeadquartersUserReader
    {
        public async Task<UserDocument> GetUserByUri(Uri headquartersUserUri)
        {
            using (var httpClient = new HttpClient())
            {
                var httpRequestMessage = new HttpRequestMessage(HttpMethod.Get, headquartersUserUri);
                httpRequestMessage.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                HttpResponseMessage response = await httpClient.SendAsync(httpRequestMessage).ConfigureAwait(false);
                string userDetailsString = await response.Content.ReadAsStringAsync().ConfigureAwait(false);

                var deserializedUserDetails = JsonConvert.DeserializeObject<UserDocument>(userDetailsString);
                return deserializedUserDetails;
            }
        }
    }
}