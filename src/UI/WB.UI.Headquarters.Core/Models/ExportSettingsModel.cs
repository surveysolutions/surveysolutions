using WB.Core.BoundedContexts.Headquarters.DataExport.Security;

namespace WB.Core.SharedKernels.SurveyManagement.Web.Models
{
    public class ExportSettingsModel
    {
        public ExportSettingsModel(){}

        public ExportSettingsModel(bool isEnabled, string password, ExportRetentionSettings exportRetentionSettings = null)
        {
            this.IsEnabled = isEnabled;
            this.Password = password;
            if (exportRetentionSettings != null)
            {
                this.IsRetentionEnabled = exportRetentionSettings.Enabled;
                this.RetentionLimitInDays = exportRetentionSettings.DaysToKeep;
                this.RetentionLimitQuantity = exportRetentionSettings.CountToKeep;
            }
        } 
        
        public bool IsEnabled { get; set; }
        public string Password { get; set; }

        public bool IsRetentionEnabled { get; set; } = false;

        public int? RetentionLimitInDays { get; set; }

        public int? RetentionLimitQuantity { get; set; }
    }
}
