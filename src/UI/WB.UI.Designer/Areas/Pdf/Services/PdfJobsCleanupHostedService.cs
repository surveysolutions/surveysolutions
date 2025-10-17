using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.Pdf;

namespace WB.UI.Designer.Areas.Pdf.Services;

public class PdfJobsCleanupHostedService : IHostedService, IDisposable
{
    private readonly PdfJobsCleanup cleanup;
    private readonly IOptions<PdfSettings> settings;
    private readonly ILogger<PdfJobsCleanupHostedService> logger;
    private Timer? timer;

    public PdfJobsCleanupHostedService(PdfJobsCleanup cleanup, IOptions<PdfSettings> settings, ILogger<PdfJobsCleanupHostedService> logger)
    {
        this.cleanup = cleanup;
        this.settings = settings;
        this.logger = logger;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        var interval = TimeSpan.FromSeconds(settings.Value.CleanupIntervalInSeconds);
        timer = new Timer(_ => SafeCleanup(), null, interval, interval);
        logger.LogInformation("PdfJobsCleanupHostedService started with interval {Interval}", interval);
        return Task.CompletedTask;
    }

    private void SafeCleanup()
    {
        try
        {
            cleanup.Cleanup();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error during PDF jobs cleanup run");
        }
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        timer?.Change(Timeout.Infinite, 0);
        return Task.CompletedTask;
    }

    public void Dispose()
    {
        timer?.Dispose();
    }
}
