using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Playwright;
using StackExchange.Exceptional;
using WB.Core.BoundedContexts.Designer;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.ChangeHistory;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.Pdf;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.Infrastructure.FileSystem;
using WB.UI.Designer.Areas.Pdf.Controllers;
using WB.UI.Designer.Areas.Pdf.Utils;
using WB.UI.Designer.Resources;
using WB.UI.Shared.Web.Services;

namespace WB.UI.Designer.Areas.Pdf.Services;

public class PdfService : IPdfService
{
    private static readonly PdfQuery pdfQuery = new PdfQuery(20, 7);

    private readonly IOptions<PdfSettings> options;
    private readonly IViewRenderService viewRenderingService;
    private readonly ILogger logger;
    private readonly IPdfFactory pdfFactory;
    private readonly IFileSystemAccessor fileSystemAccessor;
    private readonly IHttpContextAccessor httpContextAccessor;

    public PdfService(IOptions<PdfSettings> options,
        IViewRenderService viewRenderingService,
        ILogger<PdfService> logger,
        IPdfFactory pdfFactory,
        IFileSystemAccessor fileSystemAccessor,
        IHttpContextAccessor httpContextAccessor
        )
    {
        this.options = options;
        this.viewRenderingService = viewRenderingService;
        this.logger = logger;
        this.pdfFactory = pdfFactory;
        this.fileSystemAccessor = fileSystemAccessor;
        this.httpContextAccessor = httpContextAccessor;
    }

    public async Task<PdfGenerationProgress> Enqueue(QuestionnaireRevision id, Guid? translation, DocumentType documentType, int? timezoneOffsetMinutes)
    {
        var key = GetKey(documentType, id, translation);
            
        var existing = pdfQuery.GetOrNull(key);
        if (existing != null)
            return existing;

        var progress = new PdfGenerationProgress();

        string questionnaireHtml = await GetHtmlContent(id, progress, translation, timezoneOffsetMinutes ?? 0);
        string footerHtml = await RenderActionResultToString(nameof(PdfController.RenderQuestionnaireFooter), new { });
        
        Task RunGeneration(PdfGenerationProgress p) =>
            documentType == DocumentType.Pdf
                ? StartRenderPdf(questionnaireHtml, footerHtml, p)
                : StartRenderHtml(questionnaireHtml, footerHtml, p);

        PdfGenerationProgress pdfGenerationProgress = pdfQuery.GetOrAdd(key, GetUserId(), RunGeneration);

        if (pdfGenerationProgress.IsFailed)
        {
            if (timezoneOffsetMinutes == null)
            {
                pdfQuery.Remove(key);
                pdfQuery.GetOrAdd(key, GetUserId(), RunGeneration);
            }
        }
        
        return pdfGenerationProgress;
    }

    public PdfGenerationProgress? Status(QuestionnaireRevision id, Guid? translation, DocumentType documentType)
    {
        var pdfKey = GetKey(documentType, id, translation);
        return pdfQuery.GetOrNull(pdfKey);
    }

    public async Task<PdfGenerationProgress> Retry(QuestionnaireRevision id, Guid? translation, DocumentType documentType)
    {
        var key = GetKey(documentType, id, translation);

        var progress = new PdfGenerationProgress();

        string questionnaireHtml = await GetHtmlContent(id, progress, translation, 0);
        string footerHtml = await RenderActionResultToString(nameof(PdfController.RenderQuestionnaireFooter), new { });
        
        Task RunGeneration(PdfGenerationProgress p) =>
            documentType == DocumentType.Pdf
                ? StartRenderPdf(questionnaireHtml, footerHtml, p)
                : StartRenderHtml(questionnaireHtml, footerHtml, p);
        
        PdfGenerationProgress pdfGenerationProgress = pdfQuery.GetOrAdd(key, GetUserId(), RunGeneration);
        if (pdfGenerationProgress.IsFailed)
        {
            pdfQuery.Remove(key);
        }
            
        return pdfQuery.GetOrAdd(key, GetUserId(), RunGeneration);
    }

    public byte[]? Download(QuestionnaireRevision id, Guid? translation, DocumentType documentType)
    {
        var pdfKey = GetKey(documentType, id, translation);
        var pdfGenerationProgress = pdfQuery.GetOrNull(pdfKey);

        if (pdfGenerationProgress?.IsFinished == true)
        {
            byte[] content = this.fileSystemAccessor.ReadAllBytes(pdfGenerationProgress.FilePath);

            this.fileSystemAccessor.DeleteFile(pdfGenerationProgress.FilePath);
            pdfQuery.Remove(pdfKey);
            return content;
        }

        return null;
    }

    private async Task StartRenderPdf(string questionnaireHtml, string footerHtml, PdfGenerationProgress generationProgress)
    {
        if (generationProgress.IsFailed)
            return;

        try
        {
            using var playwright = await Playwright.CreateAsync();
            var browser = await playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions()
            {
                Headless = true,
                Args = new[] { "--disable-javascript" }
            });

            var page = await browser.NewPageAsync();
            await page.RouteAsync("**/*.js", async route => await route.AbortAsync());
            await page.SetContentAsync(questionnaireHtml);

            var contetnt = await page.PdfAsync(new PagePdfOptions()
            {
                HeaderTemplate = "<html></html>",
                FooterTemplate = footerHtml,
                Format = "A4",
                DisplayHeaderFooter = true,
                //Margin = new Margin() {Top = "10", Bottom = "7", Left = "0", Right = "0"},
            });
            await System.IO.File.WriteAllBytesAsync(generationProgress.FilePath, contetnt);

            generationProgress.Finish();
        }
        catch (Exception exception)
        {
            await exception.LogNoContextAsync();
            this.logger.LogError(exception, $"Failed to generate PDF");
            generationProgress.Fail();
        }
    }
    
    private async Task<string> RenderActionResultToString(string viewName, object model)
    {
        var request = httpContextAccessor.HttpContext?.Request;
        if (request == null)
            throw new Exception("HttpContext is null");
        
        var uri = new Uri($"{request.Scheme}://{request.Host}{request.PathBase}");
        string webRoot = uri.ToString().TrimEnd('/');
        string webAppRoot = uri.GetLeftPart(System.UriPartial.Authority);
        var routeData = new Microsoft.AspNetCore.Routing.RouteData();
        routeData.DataTokens.Add("area", "Pdf");
        routeData.Values.Add("controller", "Pdf");
        routeData.Values.Add("area", "Pdf");
        return await this.viewRenderingService.RenderToStringAsync(viewName, model, webRoot, webAppRoot, routeData);
    }

    private async Task StartRenderHtml(string questionnaireHtml, string footerHtml, PdfGenerationProgress progress)
    {
        if (!progress.IsFailed)
        {
            await System.IO.File.WriteAllTextAsync(progress.FilePath, questionnaireHtml);
            progress.Finish();
        }
    }
        
    private async Task<string> GetHtmlContent(QuestionnaireRevision id, PdfGenerationProgress generationProgress, Guid? translation, int timezoneOffsetMinutes)
    {
        var user = httpContextAccessor.HttpContext?.User;
        if (user == null)
            throw new Exception("HttpContext is null");
        
        PdfQuestionnaireModel? questionnaire = this.LoadQuestionnaire(id, user.GetIdOrNull(), user.GetUserNameOrNull(), translation, false);
        if (questionnaire == null)
        {
            generationProgress.Fail();
            return string.Empty;
        }
        questionnaire.TimezoneOffsetMinutes = timezoneOffsetMinutes;

        return await RenderActionResultToString(nameof(PdfController.RenderQuestionnaire), questionnaire);
    }
    
    private PdfQuestionnaireModel? LoadQuestionnaire(QuestionnaireRevision id, Guid? requestedByUserId, string? requestedByUserName, Guid? translation, bool useDefaultTranslation)
    {
        return this.pdfFactory.Load(id, requestedByUserId, requestedByUserName, translation, useDefaultTranslation);
    }

    private static string GetKey(DocumentType documentType, QuestionnaireRevision id, Guid? translation)
    {
        return documentType + ":" + id + ":" + translation;
    }

    private Guid GetUserId()
    {
        var user = httpContextAccessor.HttpContext?.User;
        if (user == null)
            return Guid.Empty;
        
        return user.GetIdOrNull() ?? Guid.Empty;
    }
}
