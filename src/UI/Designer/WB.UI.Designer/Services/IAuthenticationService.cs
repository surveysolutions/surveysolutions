namespace WB.UI.Designer.Services
{
    public interface IAuthenticationService
    {
        bool ShouldShowCaptchaByUserName(string userName);
        bool ShouldShowCaptcha();
        bool Login(string userName, string password, bool staySignedIn);
    }
}