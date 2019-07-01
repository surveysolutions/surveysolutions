using System;
using System.IO;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace support
{
    public class NetworkService : INetworkService
    {
        public async Task<bool> IsHostReachableAsync(string url)
        {
            return await GetResponseAndCheckIfNeeded(url, "HEAD");
        }

        public async Task<bool> CheckResponse(string url, string expectedResponse)
        {
            return await GetResponseAndCheckIfNeeded(url, "GET", expectedResponse);
        }

        private async Task<bool> GetResponseAndCheckIfNeeded(string url, string method ,string expectedResponse = null)
        {
            var request = (HttpWebRequest)WebRequest.Create(url);
            request.Timeout = 15000;
            request.Method = method;
            try
            {
                using (HttpWebResponse response = (HttpWebResponse)await request.GetResponseAsync())
                {
                    if (response.StatusCode != HttpStatusCode.OK)
                        return false;

                    if (expectedResponse != null)
                    {
                        var encoding = Encoding.GetEncoding(response.CharacterSet);

                        using (var responseStream = response.GetResponseStream())
                        using (var reader = new StreamReader(responseStream, encoding))
                            return string.Compare(reader.ReadToEnd(), expectedResponse, StringComparison.OrdinalIgnoreCase) == 0;
                    }
                    else
                    {
                        return true;
                    }
                }
            }
            catch (WebException)
            {
                return false;
            }
        }
    }
}
