using WB.Core.BoundedContexts.Headquarters.DataExport.Security;

namespace WB.Core.SharedKernels.SurveyManagement.Web.Models
{
    public class ExportSettingsModel
    {
        public ExportSettingsModel(){}

        public ExportSettingsModel(ExportEncryptionSettings exportEncryptionSettings, ExportRetentionSettings exportRetentionSettings = null)
        {
            if(exportEncryptionSettings != null)
            {
                this.IsEnabled = exportEncryptionSettings.IsEnabled;
                this.Password = exportEncryptionSettings.Value;
            }
            
            if (exportRetentionSettings != null)
            {
                this.IsRetentionEnabled = exportRetentionSettings.Enabled;
                this.RetentionLimitInDays = exportRetentionSettings.DaysToKeep;
                this.RetentionLimitQuantity = exportRetentionSettings.CountToKeep;
            }
        } 
        
        public bool IsEnabled { get; set; } = false;
        public string Password { get; set; }

        public bool IsRetentionEnabled { get; set; } = false;

        public int? RetentionLimitInDays { get; set; }

        public int? RetentionLimitQuantity { get; set; }
    }
}
