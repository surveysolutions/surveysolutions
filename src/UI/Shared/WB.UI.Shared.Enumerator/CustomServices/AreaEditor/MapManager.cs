using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Plugin.Permissions.Abstractions;
using WB.Infrastructure.Shared.Enumerator;

namespace WB.UI.Shared.Enumerator.CustomServices.AreaEditor
{
    public class MapService : IMapService
    {
        public MapService(IPermissions permissions)
        {
            this.permissions = permissions;
        }

        private readonly IPermissions permissions;

        private TimeSpan timeout = new TimeSpan(0,0,30);

        private string urlToCheckMaps = "https://download.mysurvey.solutions/maps.json";

        string mapsLocation = Path.Combine(Android.OS.Environment.ExternalStorageDirectory.AbsolutePath, "TheWorldBank/shared/maps/");
        string filesToSearch = "*.tpk";

        public Dictionary<string, string> GetAvailableMaps()
        {
            if (!Directory.Exists(mapsLocation))
                Directory.CreateDirectory(mapsLocation);

            var tpkFileSearchResult = Directory.GetFiles(mapsLocation, filesToSearch);

            if (tpkFileSearchResult.Length == 0)
            {
                return new Dictionary<string, string>();
            }

            return tpkFileSearchResult.ToDictionary(Path.GetFileNameWithoutExtension);
        }

        public async Task SyncMaps(CancellationToken cancellationToken)
        {
            await this.permissions.AssureHasPermission(Permission.Storage);

            try
            {
                var maps = await GetObjectFromJsonAsync<MapDescription[]>(urlToCheckMaps);

                foreach (var mapDescription in maps)
                {
                    var filename = Path.Combine(mapsLocation, mapDescription.MapName);
                    if (File.Exists(filename))
                        continue;

                    HttpClient client = new HttpClient();
                    client.Timeout = timeout;
                    var uri = new Uri(mapDescription.URL);

                    var response = await client.GetAsync(uri, cancellationToken);
                    cancellationToken.ThrowIfCancellationRequested();

                    response.EnsureSuccessStatusCode();

                    byte[] responseBytes = await response.Content.ReadAsByteArrayAsync();
                    cancellationToken.ThrowIfCancellationRequested();
                    File.WriteAllBytes(filename, responseBytes);
                    cancellationToken.ThrowIfCancellationRequested();
                }
            }
            catch (Exception exception)
            {
                
            }
        }

        private async Task<T> GetObjectFromJsonAsync<T>(string uri)
        {
            using (var httpClient = new HttpClient())
            {
                httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                var jsonData = await httpClient.GetStringAsync(uri);
                return JsonConvert.DeserializeObject<T>(jsonData);
            }
        }
    }

    public class MapDescription
    {
        public string MapName { set; get; }
        public string URL { set; get; }
    }
}