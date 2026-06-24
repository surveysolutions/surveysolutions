namespace WB.UI.Shared.Web.Services
{
    public class CaptchaConfig
    {
        public int TimespanInMinutesCaptchaWillBeShownAfterFailedLoginAttempt { get; set; } = 5;
        public int CountOfFailedLoginAttemptsBeforeCaptcha { get; set; } = 5;
        public CaptchaProviderType CaptchaType { get; set; } = CaptchaProviderType.None;
        public double RecaptchaV3MinimumScore { get; set; } = 0.5;
    }

    public enum CaptchaProviderType
    {
        None = 0,
        Recaptcha = 1,
        Hosted = 2,
        RecaptchaV3 = 3
    }
}
