namespace WB.UI.Shared.Web.Services
{
    public class CaptchaConfig
    {
        public int TimespanInMinutesCaptchaWillBeShownAfterFailedLoginAttempt { get; set; } = 5;
        public int CountOfFailedLoginAttemptsBeforeCaptcha { get; set; } = 5;
        public CaptchaProviderType CaptchaType { get; set; } = CaptchaProviderType.None;
    }

    public enum CaptchaProviderType
    {
        None, Recaptcha, Hosted
    }
}
