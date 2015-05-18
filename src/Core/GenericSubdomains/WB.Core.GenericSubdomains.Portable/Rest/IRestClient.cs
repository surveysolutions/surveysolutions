using System.Net.Http;
using System.Threading.Tasks;

namespace WB.Core.GenericSubdomains.Portable.Rest
{
    public interface IRestClient
    {
        Task<HttpResponseMessage> PostJsonAsync(object request);

        Task<HttpResponseMessage> GetAsync();

        string GetFullUrl();
    }
}