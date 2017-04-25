using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using Android.OS;
using Newtonsoft.Json;
using Plugin.Permissions.Abstractions;
using WB.Core.Infrastructure.FileSystem;
using WB.Infrastructure.Shared.Enumerator;

namespace WB.UI.Shared.Enumerator.CustomServices.AreaEditor
{
    public class MapService : IMapService
    {
        private readonly IPermissions permissions;
        private IFileSystemAccessor fileSystemAccessor;
        private TimeSpan timeout = new TimeSpan(0,0,30);
        private string urlToCheckMaps = "https://download.mysurvey.solutions/maps.json";
        private string mapsLocation;
        string filesToSearch = "*.tpk";

        public MapService(IPermissions permissions, IFileSystemAccessor fileSystemAccessor)
        {
            this.permissions = permissions;
            this.fileSystemAccessor = fileSystemAccessor;
            var pathToRootDirectory = Build.VERSION.SdkInt < BuildVersionCodes.N ? AndroidPathUtils.GetPathToExternalDirectory() : AndroidPathUtils.GetPathToInternalDirectory();
            
            //Android.OS.Environment.ExternalStorageDirectory.AbsolutePath

            mapsLocation = fileSystemAccessor.CombinePath(pathToRootDirectory, "TheWorldBank/Shared/MapCache/");
        }

        public Dictionary<string, string> GetAvailableMaps()
        {

            if (!fileSystemAccessor.IsDirectoryExists(mapsLocation))
                return new Dictionary<string, string>();

            var tpkFileSearchResult = fileSystemAccessor.GetFilesInDirectory(mapsLocation, filesToSearch).OrderBy(x => x).ToList();
            if (tpkFileSearchResult.Count == 0)
                return new Dictionary<string, string>();

            return tpkFileSearchResult.ToDictionary(this.fileSystemAccessor.GetFileNameWithoutExtension);
        }

        public async Task SyncMaps(CancellationToken cancellationToken)
        {
            await this.permissions.AssureHasPermission(Permission.Storage);

            if (!fileSystemAccessor.IsDirectoryExists(mapsLocation))
                fileSystemAccessor.CreateDirectory(mapsLocation);

            try
            {
                var maps = await GetObjectFromJsonAsync<MapDescription[]>(urlToCheckMaps);

                foreach (var mapDescription in maps)
                {
                    var filename = fileSystemAccessor.CombinePath(mapsLocation, mapDescription.MapName);

                    if (fileSystemAccessor.IsFileExists(filename))
                        continue;

                    HttpClient client = new HttpClient();
                    client.Timeout = timeout;
                    var uri = new Uri(mapDescription.URL);

                    var response = await client.GetAsync(uri, cancellationToken);
                    cancellationToken.ThrowIfCancellationRequested();

                    response.EnsureSuccessStatusCode();

                    byte[] responseBytes = await response.Content.ReadAsByteArrayAsync();
                    cancellationToken.ThrowIfCancellationRequested();
                    fileSystemAccessor.WriteAllBytes(filename, responseBytes);
                    cancellationToken.ThrowIfCancellationRequested();
                }
            }
            catch (Exception)
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