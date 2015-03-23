using System;
using System.Threading;
using System.Threading.Tasks;
using WB.Core.GenericSubdomains.Utils.Implementation;

namespace WB.Core.GenericSubdomains.Utils.Services
{
    public interface IRestService
    {
        Task<T> GetAsync<T>(string url, object queryString = null, RestCredentials credentials = null);
        Task GetAsync(string url, object queryString = null, RestCredentials credentials = null);
        Task<T> PostAsync<T>(string url, object request = null, RestCredentials credentials = null);
        Task PostAsync(string url, object request = null, RestCredentials credentials = null);


        Task<T> GetAsync<T>(string url, CancellationToken token,  object queryString = null, RestCredentials credentials = null);
        Task GetAsync(string url, CancellationToken token, object queryString = null, RestCredentials credentials = null);
        Task<T> PostAsync<T>(string url, CancellationToken token, object request = null, RestCredentials credentials = null);
        Task PostAsync(string url, CancellationToken token, object request = null, RestCredentials credentials = null);

        Task<T> GetWithProgressAsync<T>(string url, CancellationToken token, Action<decimal> progressPercentage,  object queryString = null, RestCredentials credentials = null);
        Task<T> PostWithProgressAsync<T>(string url, CancellationToken token, Action<decimal> progressPercentage, object request = null, RestCredentials credentials = null);
    }
}