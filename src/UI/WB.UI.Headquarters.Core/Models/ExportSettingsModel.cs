using WB.Core.BoundedContexts.Headquarters.DataExport.Security;

namespace WB.Core.SharedKernels.SurveyManagement.Web.Models
{
    public class ExportSettingsModel
    {
        public ExportSettingsModel(){}

        public ExportSettingsModel(ExportEncryptionSettings exportEncryptionSettings, 
            ExportRetentionSettings exportRetentionSettings = null,
            GeographyExportFormat geographyExportFormat = GeographyExportFormat.Wkt)
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

            this.GeographyExportFormat = geographyExportFormat;
        } 
        
        public bool IsEnabled { get; set; } = false;
        public string Password { get; set; }

        public bool IsRetentionEnabled { get; set; } = false;

        public int? RetentionLimitInDays { get; set; }

        public int? RetentionLimitQuantity { get; set; }

        public GeographyExportFormat GeographyExportFormat { get; set; } = GeographyExportFormat.Wkt;
    }
}
