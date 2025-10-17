using System;
using System.IO;
using System.Linq;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.Pdf;
using WB.UI.Designer.Areas.Pdf.Utils;

namespace WB.UI.Designer.Areas.Pdf.Services;

public class PdfJobsCleanup
{
    private readonly IPdfQuery pdfQuery;
    private readonly IOptions<PdfSettings> settings;
    private readonly ILogger<PdfJobsCleanup> logger;

    public PdfJobsCleanup(IPdfQuery pdfQuery, IOptions<PdfSettings> settings, ILogger<PdfJobsCleanup> logger)
    {
        this.pdfQuery = pdfQuery;
        this.settings = settings;
        this.logger = logger;
    }

    public void Cleanup()
    {
        var jobInfos = pdfQuery.GetOldJobs();
        foreach (var jobInfo in jobInfos)
        {
            try
            {
                if (File.Exists(jobInfo.FilePath))
                {
                    File.Delete(jobInfo.FilePath);
                    logger.LogDebug("Deleted stale PDF file for job {Key} at {Path}", jobInfo.Key, jobInfo.FilePath);
                }
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "Failed deleting PDF file {Path} for job {Key}", jobInfo.FilePath, jobInfo.Key);
            }
            pdfQuery.Remove(jobInfo.Key);
        }
    }
}

