using System.Threading;
using System.Threading.Tasks;
using WB.Core.GenericSubdomains.Utils.Implementation.Services.Rest;

namespace WB.Core.GenericSubdomains.Utils.Services.Rest
{
    public interface IRestService
    {
        Task<T> GetAsync<T>(string url, dynamic requestData = null, RestCredentials credentials = null,
            CancellationToken token = default(CancellationToken));

        Task GetAsync(string url, dynamic requestData = null, RestCredentials credentials = null,
            CancellationToken token = default(CancellationToken));

        Task<T> PostAsync<T>(string url, dynamic requestData = null, RestCredentials credentials = null,
            CancellationToken token = default(CancellationToken));

        Task PostAsync(string url, dynamic requestData = null, RestCredentials credentials = null,
            CancellationToken token = default(CancellationToken));
    }
}