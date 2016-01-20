using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
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

        public void RunScavenge()
        {
            UriBuilder uriBuilder = new UriBuilder("http", this.settings.ServerIP, this.settings.ServerHttpPort, "/admin/scavenge");
            var scanvageUri = uriBuilder.ToString();

            using (var client = new HttpClient())
            {
                var byteArray = Encoding.ASCII.GetBytes(this.settings.Login + ":" + this.settings.Password);
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", Convert.ToBase64String(byteArray));

                var response = client.PostAsync(scanvageUri, null).WaitAndUnwrapException();
                var responseString = response.Content.ReadAsStringAsync().WaitAndUnwrapException();
            }
        }
    }
}