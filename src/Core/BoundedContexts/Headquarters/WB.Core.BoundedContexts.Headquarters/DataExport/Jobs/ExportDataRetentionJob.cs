using Quartz;

namespace WB.Core.BoundedContexts.Headquarters.DataExport.Jobs;

[DisallowConcurrentExecution]
[DisplayName("Export Dara Retention")]
[Category("Cleanup")]
public class ExportDataRetentionJob : IJob
{
    private readonly ILogger<ExportDataRetentionJob> logger;

    public SendInterviewCompletedJob(
        ILogger<ExportDataRetentionJob> logger
    )
    {
        this.logger = logger;
    }
    
    public async Task Execute(IJobExecutionContext context)
    {
        try
        {
            this.logger.LogInformation("Export retention job: STARTED");
            
            
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
