using WB.Core.BoundedContexts.Headquarters.Views;

namespace WB.Core.BoundedContexts.Headquarters.DataExport;

public class ExportRetentionSettings: AppSetting
{
    public bool Enabled { get; set; }
    
    public int? RetentionLimitInDays { get; set; }
    public int? RetentionLimitQuantity { get; set; }
}
