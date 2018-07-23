using System.Threading.Tasks;
using WB.Core.GenericSubdomains.Portable.ServiceLocation;
using WB.Core.Infrastructure.Modularity;
using WB.UI.Shared.Web.Configuration;

namespace WB.UI.Shared.Web.Captcha
{
    public class CaptchaModule : IModule
    {
        private readonly string captcha;

        public CaptchaModule(string captcha)
        {
            this.captcha = captcha;
        }

        public void Load(IIocRegistry registry)
        {
            if (string.IsNullOrWhiteSpace(captcha))
            {
                registry.Bind<ICaptchaProvider, NoCaptchaProvider>();
            }
            else
            {
                switch (captcha.ToLowerInvariant())
                {
                    case "recaptcha":
                        registry.Bind<ICaptchaProvider, ReCaptchaProvider>();
                        break;
                    case "hosted":
                        registry.Bind<ICaptchaProvider, HostedCaptchaProvider>();
                        break;
                    default:
                        registry.Bind<ICaptchaProvider, NoCaptchaProvider>();
                        break;
                }
            }

            registry.Bind<ICaptchaService, WebCacheBasedCaptchaService>();
        }

        public Task Init(IServiceLocator serviceLocator, UnderConstructionInfo status)
        {
            return Task.CompletedTask;
        }
    }
}
