using System.Threading;
using System.Threading.Tasks;
using WB.Core.SharedKernel.Utils.Implementation.Services;

namespace WB.Core.SharedKernel.Utils.Services.Rest
{
    public interface IRestService
    {
        Task<T> GetAsync<T>(string url, dynamic requestBody = null, dynamic requestQueryString = null, RestCredentials credentials = null,
            CancellationToken token = default(CancellationToken));

        Task GetAsync(string url, dynamic requestBody = null, dynamic requestQueryString = null, RestCredentials credentials = null,
            CancellationToken token = default(CancellationToken));

        Task<T> PostAsync<T>(string url, dynamic requestBody = null, dynamic requestQueryString = null, RestCredentials credentials = null,
            CancellationToken token = default(CancellationToken));

        Task PostAsync(string url, dynamic requestBody = null, dynamic requestQueryString = null, RestCredentials credentials = null,
            CancellationToken token = default(CancellationToken));
    }
}