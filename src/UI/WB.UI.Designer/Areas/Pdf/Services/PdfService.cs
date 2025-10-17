using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Playwright;
using StackExchange.Exceptional;
using WB.Core.BoundedContexts.Designer;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.ChangeHistory;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.Pdf;
using WB.Core.Infrastructure.FileSystem;
using WB.UI.Designer.Areas.Pdf.Controllers;
using WB.UI.Designer.Areas.Pdf.Utils;
using WB.UI.Shared.Web.Services;

namespace WB.UI.Designer.Areas.Pdf.Services;

public class PdfService : IPdfService
{
    private readonly IViewRenderService viewRenderingService;
    private readonly ILogger logger;
    private readonly IPdfFactory pdfFactory;
    private readonly IFileSystemAccessor fileSystemAccessor;
    private readonly IHttpContextAccessor httpContextAccessor;
    private readonly IPdfQuery pdfQuery;
    private readonly IPdfRender pdfRender;
    
    public PdfService(
        IViewRenderService viewRenderingService,
        ILogger<PdfService> logger,
        IPdfFactory pdfFactory,
        IFileSystemAccessor fileSystemAccessor,
        IHttpContextAccessor httpContextAccessor,
        IPdfQuery pdfQuery,
        IPdfRender pdfRender
        )
    {
        this.viewRenderingService = viewRenderingService;
        this.logger = logger;
        this.pdfFactory = pdfFactory;
        this.fileSystemAccessor = fileSystemAccessor;
        this.httpContextAccessor = httpContextAccessor;
        this.pdfQuery = pdfQuery;
        this.pdfRender = pdfRender;
    }
    
    public async Task<byte[]> GetHtmlContent(QuestionnaireRevision id, Guid? translation)
    {
        var progress = new PdfGenerationProgress();
        string questionnaireHtml = await GetHtmlContent(id, progress, translation, 0);
        return Encoding.UTF8.GetBytes(questionnaireHtml);
    }

    public async Task<PdfGenerationProgress> Enqueue(QuestionnaireRevision id, Guid? translation, DocumentType documentType, int? timezoneOffsetMinutes)
    {
        var key = GetKey(documentType, id, translation);
            
        var existing = pdfQuery.GetOrNull(key);
        if (existing != null)
            pdfQuery.Remove(key);
        
        var progress = new PdfGenerationProgress();

        string questionnaireHtml = await GetHtmlContent(id, progress, translation, timezoneOffsetMinutes ?? 0);
        string footerHtml = await RenderActionResultToString(nameof(PdfController.RenderQuestionnaireFooter), new { });
        
        Task RunGeneration(PdfGenerationProgress p, CancellationToken token) =>
            documentType == DocumentType.Pdf
                ? StartRenderPdf(questionnaireHtml, footerHtml, p, token)
                : StartRenderHtml(questionnaireHtml, p, token);

        PdfGenerationProgress pdfGenerationProgress = pdfQuery.GetOrAdd(GetUserId(), key, RunGeneration);
        return pdfGenerationProgress;
    }

    public PdfGenerationProgress? Status(QuestionnaireRevision id, Guid? translation, DocumentType documentType)
    {
        var pdfKey = GetKey(documentType, id, translation);
        return pdfQuery.GetOrNull(pdfKey);
    }


    public byte[]? Download(QuestionnaireRevision id, Guid? translation, DocumentType documentType)
    {
        var pdfKey = GetKey(documentType, id, translation);
        var pdfGenerationProgress = pdfQuery.GetOrNull(pdfKey);

        if (pdfGenerationProgress?.Status == PdfGenerationStatus.Finished)
        {
            byte[] content = this.fileSystemAccessor.ReadAllBytes(pdfGenerationProgress.FilePath);

            this.fileSystemAccessor.DeleteFile(pdfGenerationProgress.FilePath);
            pdfQuery.Remove(pdfKey);
            return content;
        }

        return null;
    }

    public string GetCurrentInfoJson() => pdfQuery.GetQueryInfoJson();


    private async Task StartRenderPdf(string questionnaireHtml, string footerHtml, PdfGenerationProgress generationProgress, CancellationToken token)
    {
        if (generationProgress.Status == PdfGenerationStatus.Failed)
            return;

        try
        {
            var content = await pdfRender.RenderPdf(questionnaireHtml, footerHtml, token).ConfigureAwait(false);
            await this.fileSystemAccessor.WriteAllBytesAsync(generationProgress.FilePath, content, token).ConfigureAwait(false);
            
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
        string webAppRoot = uri.GetLeftPart(UriPartial.Authority);
        var routeData = new Microsoft.AspNetCore.Routing.RouteData();
        routeData.DataTokens.Add("area", "Pdf");
        routeData.Values.Add("controller", "Pdf");
        routeData.Values.Add("area", "Pdf");
        return await this.viewRenderingService.RenderToStringAsync(viewName, model, webRoot, webAppRoot, routeData);
    }

    private async Task StartRenderHtml(string questionnaireHtml, PdfGenerationProgress progress, CancellationToken _)
    {
        if (progress.Status != PdfGenerationStatus.Failed)
        {
            await this.fileSystemAccessor.WriteAllTextAsync(progress.FilePath, questionnaireHtml);
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

    private string GetKey(DocumentType documentType, QuestionnaireRevision id, Guid? translation)
    {
        return GetUserId() + ":" + documentType + ":" + id + ":" + translation;
    }

    private Guid GetUserId()
    {
        var user = httpContextAccessor.HttpContext?.User;
        if (user == null)
            return Guid.Empty;
        
        return user.GetIdOrNull() ?? Guid.Empty;
    }
}
