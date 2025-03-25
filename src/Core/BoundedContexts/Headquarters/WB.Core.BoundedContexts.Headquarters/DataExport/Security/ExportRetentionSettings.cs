using WB.Core.BoundedContexts.Headquarters.Views;

namespace WB.Core.BoundedContexts.Headquarters.DataExport.Security;

public class ExportRetentionSettings: AppSetting
{
    public ExportRetentionSettings(bool enabled, int? daysToKeep, int? countToKeep)
    {
        this.Enabled = enabled;
        this.DaysToKeep = daysToKeep;
        this.CountToKeep = countToKeep;
    }

    public bool Enabled { get; set; }
    
    public int? DaysToKeep { get; set; }
    public int? CountToKeep { get; set; }
}
