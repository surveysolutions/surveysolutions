using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure;

namespace WB.Core.BoundedContexts.Tester.Implementation.Services
{
    public class TesterMapSynchronizer : IMapSynchronizer
    {
        private string baseMapsUrl = "https://download.mysurvey.solutions/";
        private string mapsListUrl = "configuration/maps.json";
        
        public async Task<List<MapView>> GetMapList(CancellationToken cancellationToken)
        {
            using (var httpClient = new HttpClient())
            {
                httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                var jsonData = await httpClient.GetStringAsync(baseMapsUrl + mapsListUrl).ConfigureAwait(false);
                return JsonConvert.DeserializeObject<List<MapView>>(jsonData);
            }
        }

        public async Task<byte[]> GetMapContent(string url, CancellationToken cancellationToken)
        {
            HttpClient client = new HttpClient();
            var uri = new Uri(baseMapsUrl + url);

            var response = await client.GetAsync(uri, cancellationToken).ConfigureAwait(false);
            cancellationToken.ThrowIfCancellationRequested();

            response.EnsureSuccessStatusCode();

            return await response.Content.ReadAsByteArrayAsync();
        }
    }
    
}

