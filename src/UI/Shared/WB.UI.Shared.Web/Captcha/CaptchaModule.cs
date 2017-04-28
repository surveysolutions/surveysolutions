using Ninject;
using Ninject.Modules;
using WB.UI.Shared.Web.Configuration;

namespace WB.UI.Shared.Web.Captcha
{
    public class CaptchaModule : NinjectModule
    {
        public override void Load()
        {
            var configuration = this.Kernel.Get<IConfigurationManager>().AppSettings;

            var captcha = configuration.Get("CaptchaService");

            if (string.IsNullOrWhiteSpace(captcha))
            {
                this.Bind<ICaptchaProvider>().To<NoCaptchaProvider>();
            }
            else
            {
                switch (captcha.ToLowerInvariant())
                {
                    case "recaptcha": this.Bind<ICaptchaProvider>().To<ReCaptchaProvider>();
                        break;
                    case "hosted":
                        this.Bind<ICaptchaProvider>().To<HostedCaptchaProvider>();
                        break;
                    default:
                        this.Bind<ICaptchaProvider>().To<NoCaptchaProvider>();
                        break;
                }
            }

            this.Bind<ICaptchaService>().To<WebCacheBasedCaptchaService>();
        }
    }
}