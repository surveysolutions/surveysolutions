namespace WB.Core.Infrastructure.Security
{
    public class Setting 
    {
        public static readonly string EncriptionSettingId = "encryption";

        public Setting(bool isEnabled, string value)
        {
            this.Value = value;
            this.IsEnabled = isEnabled;
        }

        public bool IsEnabled { get; private set; }
        public string Value { get; private set; }
    }
}