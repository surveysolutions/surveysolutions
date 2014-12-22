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
    }
}