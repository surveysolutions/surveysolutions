using reCAPTCHA.AspNetCore;

namespace WB.UI.Designer.Models
{
    public class CaptchaConfig : RecaptchaSettings
    {
        public CaptchaConfig()
        {
            this.TimespanInMinutesCaptchaWillBeShownAfterFailedLoginAttempt = 5;
            this.CountOfFailedLoginAttemptsBeforeCaptcha = 5;
        }

        public int TimespanInMinutesCaptchaWillBeShownAfterFailedLoginAttempt { get; set; }
        public int CountOfFailedLoginAttemptsBeforeCaptcha { get; set; }
        public bool IsReCaptchaEnabled { get; set; }
    }
}
