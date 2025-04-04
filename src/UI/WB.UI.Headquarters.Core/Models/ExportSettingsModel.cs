namespace WB.Core.SharedKernels.SurveyManagement.Web.Models
{
    public class ExportSettingsModel
    {
        public ExportSettingsModel(){}

        public ExportSettingsModel(bool isEnabled, string password, bool isRetentionEnabled = false, 
            int? retentionLimitInDays = null, 
            int? retentionLimitQuantity = null)
        {
            this.IsEnabled = isEnabled;
            this.Password = password;
            this.IsRetentionEnabled = isRetentionEnabled;
            this.RetentionLimitInDays = retentionLimitInDays;
            this.RetentionLimitQuantity = retentionLimitQuantity;
        }

        public bool IsEnabled { get; set; }
        public string Password { get; set; }
        
        public bool IsRetentionEnabled { get; set; }

        public int? RetentionLimitInDays { get; set; }

        public int? RetentionLimitQuantity { get; set; }
    }
}
