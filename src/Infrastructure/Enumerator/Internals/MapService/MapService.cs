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

namespace WB.Infrastructure.Shared.Enumerator.Internals.MapService
{
    public class MapService : IMapService
    {
        private readonly IPermissions permissions;
        private IFileSystemAccessor fileSystemAccessor;
        private TimeSpan timeout = new TimeSpan(0,0,30);

        private string urlToCheckMaps;

        private string mapsLocation;
        string filesToSearch = "*.tpk";
        private string mapsListFile = "/configuration/maps.config";

        public MapService(IPermissions permissions, 
            IFileSystemAccessor fileSystemAccessor,
            string urlToCheckMaps)
        {
            this.permissions = permissions;
            this.fileSystemAccessor = fileSystemAccessor;
            this.urlToCheckMaps = urlToCheckMaps;
            var pathToRootDirectory = Build.VERSION.SdkInt < BuildVersionCodes.N ? AndroidPathUtils.GetPathToExternalDirectory() : AndroidPathUtils.GetPathToInternalDirectory();
            
            this.mapsLocation = fileSystemAccessor.CombinePath(pathToRootDirectory, "TheWorldBank/Shared/MapCache/");
        }

        public Dictionary<string, string> GetAvailableMaps()
        {

            if (!this.fileSystemAccessor.IsDirectoryExists(this.mapsLocation))
                return new Dictionary<string, string>();

            var tpkFileSearchResult = this.fileSystemAccessor.GetFilesInDirectory(this.mapsLocation, this.filesToSearch).OrderBy(x => x).ToList();
            if (tpkFileSearchResult.Count == 0)
                return new Dictionary<string, string>();

            return tpkFileSearchResult.ToDictionary(this.fileSystemAccessor.GetFileNameWithoutExtension);
        }

        public async Task SyncMaps(CancellationToken cancellationToken)
        {
            await this.permissions.AssureHasPermission(Permission.Storage);

            if (!this.fileSystemAccessor.IsDirectoryExists(this.mapsLocation))
                this.fileSystemAccessor.CreateDirectory(this.mapsLocation);

            try
            {
                var maps = await this.GetObjectFromJsonAsync<MapDescription[]>(this.urlToCheckMaps);

                foreach (var mapDescription in maps)
                {
                    var filename = this.fileSystemAccessor.CombinePath(this.mapsLocation, mapDescription.MapName);

                    if (this.fileSystemAccessor.IsFileExists(filename))
                        continue;

                    HttpClient client = new HttpClient();
                    client.Timeout = this.timeout;
                    var uri = new Uri(mapDescription.URL);

                    var response = await client.GetAsync(uri, cancellationToken);
                    cancellationToken.ThrowIfCancellationRequested();

                    response.EnsureSuccessStatusCode();

                    byte[] responseBytes = await response.Content.ReadAsByteArrayAsync();
                    cancellationToken.ThrowIfCancellationRequested();
                    this.fileSystemAccessor.WriteAllBytes(filename, responseBytes);
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
                var url = uri + mapsListFile;
                var jsonData = await httpClient.GetStringAsync(url);
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