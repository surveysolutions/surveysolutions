namespace WB.Core.SharedKernels.SurveyManagement.Web.Models
{
    public class ExportSettingsModel
    {
        public ExportSettingsModel(){}

        public ExportSettingsModel(bool isEnabled, string password)
        {
            this.IsEnabled = isEnabled;
            this.Password = password;
        }

        public bool IsEnabled { get; set; }
        public string Password { get; set; }
    }
}
