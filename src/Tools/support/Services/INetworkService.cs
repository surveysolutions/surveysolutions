using System.Threading.Tasks;

namespace support
{
    public interface INetworkService
    {
        Task<bool> IsHostReachableAsync(string url);
        Task<bool> CheckResponse(string url, string expectedResponse);
    }
}
