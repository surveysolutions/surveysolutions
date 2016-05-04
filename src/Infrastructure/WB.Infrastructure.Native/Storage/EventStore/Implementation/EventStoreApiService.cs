using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Nito.AsyncEx.Synchronous;
using WB.Core.GenericSubdomains.Portable.Tasks;

namespace WB.Infrastructure.Native.Storage.EventStore.Implementation
{
    public class EventStoreApiService : IEventStoreApiService
    {
        private readonly EventStoreSettings settings;

        public EventStoreApiService(EventStoreSettings settings)
        {
            this.settings = settings;
        }

        public async Task RunScavengeAsync()
        {
            UriBuilder uriBuilder = new UriBuilder("http", this.settings.ServerIP, this.settings.ServerHttpPort, "/admin/scavenge");
            var scanvageUri = uriBuilder.ToString();

            using (var client = new HttpClient())
            {
                var byteArray = Encoding.ASCII.GetBytes(this.settings.Login + ":" + this.settings.Password);
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", Convert.ToBase64String(byteArray));

                var response = await client.PostAsync(scanvageUri, null);
                var responseString = await response.Content.ReadAsStringAsync();
            }
        }
    }
}