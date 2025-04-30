using Microsoft.Extensions.Logging;
using Quartz;
using System;
using System.ComponentModel;
using System.Threading.Tasks;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.BoundedContexts.Headquarters.DataExport.Security;

namespace WB.Core.BoundedContexts.Headquarters.DataExport.Jobs;

[DisallowConcurrentExecution]
[DisplayName("Export Data Retention")]
[Category("Cleanup")]
public class ExportDataRetentionJob : IJob
{
    private readonly IExportServiceApi exportServiceApi;
    private readonly ILogger<ExportDataRetentionJob> logger;
    private readonly IExportSettings exportSettings;

    public ExportDataRetentionJob(
        IExportSettings exportSettings,
        IExportServiceApi exportServiceApi,
        ILogger<ExportDataRetentionJob> logger
    )
    {
        this.logger = logger;
        this.exportServiceApi = exportServiceApi;
        this.exportSettings = exportSettings;
    }
    
    public async Task Execute(IJobExecutionContext context)
    {
        try
        {
            //this.logger.LogInformation("Export retention job: STARTED");
            var exportRetentionSettings = this.exportSettings.GetExportRetentionSettings();
            if (exportRetentionSettings?.Enabled != true)
                return;
            
            //exportServiceApi calls to delete old exports
            await exportServiceApi.RunRetentionPolicy(exportRetentionSettings.CountToKeep, exportRetentionSettings.DaysToKeep);
        }
        catch (OperationCanceledException)
        {
            this.logger.LogWarning("Export retention job: CANCELED");
        }
        catch (Exception ex)
        {
            this.logger.LogError(ex, "Export retention job: FAILED");
        }
    }
}
