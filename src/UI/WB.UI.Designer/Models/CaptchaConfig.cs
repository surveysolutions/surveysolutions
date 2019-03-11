namespace WB.UI.Designer.Models
{
    public class CaptchaConfig
    {
        public CaptchaConfig()
        {
            this.TimespanInMinutesCaptchaWillBeShownAfterFailedLoginAttempt = 5;
            this.CountOfFailedLoginAttemptsBeforeCaptcha = 5;
        }

        public int TimespanInMinutesCaptchaWillBeShownAfterFailedLoginAttempt { get; set; }
        public int CountOfFailedLoginAttemptsBeforeCaptcha { get; set; }
        public string RecaptchaPrivateKey { get; set; }
        public string RecaptchaPublicKey { get; set; }
    }
}
