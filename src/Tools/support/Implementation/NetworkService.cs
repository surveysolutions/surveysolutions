using System.Net;
using System.Threading.Tasks;

namespace support
{
    public class NetworkService : INetworkService
    {
        public async Task<bool> IsHostReachableAsync(string url)
        {
            var request = (HttpWebRequest)WebRequest.Create(url);
            request.Timeout = 15000;
            request.Method = "HEAD";
            try
            {
                using (HttpWebResponse response = (HttpWebResponse)await request.GetResponseAsync())
                {
                    return response.StatusCode == HttpStatusCode.OK;
                }
            }
            catch (WebException)
            {
                return false;
            }
        }
    }
}