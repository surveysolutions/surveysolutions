
namespace WB.UI.Designer
{
    public sealed class AppSettings: WebConfigHelper
    {
        public static readonly AppSettings Instance = new AppSettings();

        const string ISRECATPCHAENABLED = "IsReCaptchaEnabled";

        public bool IsReCaptchaEnabled { get; private set; }

        private AppSettings()
        {
            IsReCaptchaEnabled = GetBoolean(ISRECATPCHAENABLED, true);
        }
    }
}