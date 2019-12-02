using Microsoft.AspNetCore.Mvc;

namespace WB.UI.Shared.Web.Captcha
{
    public interface ICaptchaProvider
    {
        bool IsCaptchaValid(Controller controller);
    }

    public class NoCaptchaProvider : ICaptchaProvider
    {
        public bool IsCaptchaValid(Controller controller)
        {
            return true;
        }
    }
}
