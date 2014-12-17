using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using WB.Core.GenericSubdomains.Utils.Implementation;

namespace WB.Core.GenericSubdomains.Utils.Services
{
    public interface IRestService
    {
        Task<T> GetAsync<T>(string url, dynamic request = null, RestCredentials credentials = null, CancellationToken token = default(CancellationToken));

        Task GetAsync(string url, dynamic request = null, RestCredentials credentials = null, CancellationToken token = default(CancellationToken));

        Task<T> PostAsync<T>(string url, dynamic request = null, RestCredentials credentials = null, CancellationToken token = default(CancellationToken));

        Task PostAsync(string url, dynamic request = null, RestCredentials credentials = null, CancellationToken token = default(CancellationToken));

        Task PostWithAttachmentsAsync(string url, IEnumerable<RestAttachment> attachments, dynamic request = null, RestCredentials credentials = null,
            CancellationToken token = new CancellationToken());
    }
}