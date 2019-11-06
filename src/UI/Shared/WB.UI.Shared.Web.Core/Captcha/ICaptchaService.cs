namespace WB.UI.Shared.Web.Captcha
{
    public interface ICaptchaService
    {
        bool ShouldShowCaptcha(string username);
        void RegisterFailedLogin(string username);
        void ResetFailedLogin(string username);
    }
}