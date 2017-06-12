using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using WB.Core.Infrastructure.FileSystem;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure;

namespace WB.Core.BoundedContexts.Tester.Implementation.Services
{
    public class TesterMapSynchronizer : IMapSynchronizer
    {
        private readonly IFileSystemAccessor fileSystemAccessor;
        private string urlToCheckMaps = "https://download.mysurvey.solutions/configuration/maps.config";
        public TesterMapSynchronizer(IFileSystemAccessor fileSystemAccessor)
        {
            this.fileSystemAccessor = fileSystemAccessor;
        }
        
        public async Task SyncMaps(string workingDirectory, CancellationToken cancellationToken)
        {
            if (!this.fileSystemAccessor.IsDirectoryExists(workingDirectory))
                this.fileSystemAccessor.CreateDirectory(workingDirectory);
            
             var maps = await this.GetObjectFromJsonAsync<MapDescription[]>();

             foreach (var mapDescription in maps)
             {
                    var filename = this.fileSystemAccessor.CombinePath(workingDirectory, mapDescription.MapName);

                    if (this.fileSystemAccessor.IsFileExists(filename))
                        continue;

                    HttpClient client = new HttpClient();
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

        private async Task<T> GetObjectFromJsonAsync<T>()
        {
            using (var httpClient = new HttpClient())
            {
                httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                var jsonData = await httpClient.GetStringAsync(urlToCheckMaps);
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

