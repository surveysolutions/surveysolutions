using System.Net.Http;
using System.Net.Http.Headers;

namespace WB.Core.BoundedContexts.Supervisor.Extensions
{
    public static class WebClientExtensions
    {
        public static HttpClient AppendAuthToken(this HttpClient client, IHeadquartersSettings headquartersSettings)
        {
            if (!string.IsNullOrWhiteSpace(headquartersSettings.AccessToken))
            {
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(headquartersSettings.AccessToken);
            }

            return client;
        }
    }
}