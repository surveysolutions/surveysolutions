using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace WB.UI.Shared.Web.Captcha
{
    public interface ICaptchaProvider
    {
        Task<bool> IsCaptchaValid(HttpRequest request);
    }

    public class NoCaptchaProvider : ICaptchaProvider
    {
        public Task<bool> IsCaptchaValid(HttpRequest request)
        {
            return Task.FromResult(true);
        }
    }

}
