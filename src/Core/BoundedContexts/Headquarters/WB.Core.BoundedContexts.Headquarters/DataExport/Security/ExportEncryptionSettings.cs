namespace WB.Core.BoundedContexts.Headquarters.DataExport.Security
{
    public class ExportEncryptionSettings
    {
        public static readonly string EncriptionSettingId = "exportencryptionsettings";

        public ExportEncryptionSettings(bool isEnabled, string value)
        {
            this.Value = value;
            this.IsEnabled = isEnabled;
        }

        public bool IsEnabled { get; private set; }
        public string Value { get; private set; }
    }
}