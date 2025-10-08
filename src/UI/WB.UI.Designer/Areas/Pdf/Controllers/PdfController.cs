using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Playwright;
using StackExchange.Exceptional;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.Pdf;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.Infrastructure.FileSystem;
using WB.UI.Designer.Resources;
using WB.Core.BoundedContexts.Designer;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.ChangeHistory;
using WB.UI.Designer.Areas.Pdf.Services;
using WB.UI.Designer.Areas.Pdf.Utils;
using WB.UI.Designer.Controllers.Api.Designer;
using WB.UI.Shared.Web.Services;
using HtmlRenderer = Markdig.Renderers.HtmlRenderer;

namespace WB.UI.Designer.Areas.Pdf.Controllers
{
    [Area("Pdf")]
    [Route("pdf")]
    [AuthorizeOrAnonymousQuestionnaire]
    [QuestionnairePermissions]
    public class PdfController : Controller
    {
        private readonly IPdfFactory pdfFactory;
        private readonly ILogger logger;
        private readonly IFileSystemAccessor fileSystemAccessor;
        private readonly IPdfService pdfService;

        public PdfController(
            IPdfFactory pdfFactory, 
            ILogger<PdfController> logger,
            IFileSystemAccessor fileSystemAccessor,
            IPdfService pdfService)
        {
            this.pdfFactory = pdfFactory;
            this.pdfService = pdfService;
            this.logger = logger;
            this.fileSystemAccessor = fileSystemAccessor;
        }

        internal IActionResult RenderQuestionnaire(QuestionnaireRevision id, Guid? requestedByUserId, string? requestedByUserName, Guid? translation, string cultureCode, int timezoneOffsetMinutes)
        {
            if (!string.IsNullOrWhiteSpace(cultureCode))
            {
                CultureInfo culture = CultureInfo.GetCultureInfo(cultureCode);
                Thread.CurrentThread.CurrentCulture = culture;
                Thread.CurrentThread.CurrentUICulture = culture;
            }
            PdfQuestionnaireModel? questionnaire = this.pdfFactory.Load(id, requestedByUserId, requestedByUserName, translation, false);;
            if (questionnaire == null)
            {
                return StatusCode((int)HttpStatusCode.NotFound);
            }
            questionnaire.TimezoneOffsetMinutes = timezoneOffsetMinutes;
            return this.View("RenderQuestionnaire", questionnaire);
        }

        internal ActionResult RenderQuestionnaireFooter()
        {
            return this.View("RenderQuestionnaireFooter");
        }

        [Route("printpreview/{id}")]
        public IActionResult PrintPreview(QuestionnaireRevision id, Guid? translation)
        {
            PdfQuestionnaireModel? questionnaire = this.pdfFactory.Load(id, User.GetIdOrNull(), User.GetUserNameOrNull(), translation, false);;
            if (questionnaire == null)
            {
                return StatusCode((int)HttpStatusCode.NotFound);
            }
            return this.View("RenderQuestionnaire", questionnaire);
        }
        
        //override old methods to supply html to heeaquarters instead of pdf
        
        [ResponseCache(Duration = 0, NoStore = true)]
        [Route("download/{id}")]
        public IActionResult Download(QuestionnaireRevision id, Guid? translation)
        {
            var content = pdfService.Download(id, translation, DocumentType.Html);

            if (content != null)
            {
                // MS edge brakes on long file name
                var questionnaireTitle = this.pdfFactory.LoadQuestionnaireTitle(id.QuestionnaireId);
                string? validTitle = (questionnaireTitle?.Length < 250 ? questionnaireTitle : questionnaireTitle?.Substring(0, 250) ?? "questionnaire");

                return this.File(content, "application/html", $"{validTitle}.html");
            }
            else
            {
                return StatusCode((int)HttpStatusCode.NotFound);
            }
        }

        [ResponseCache (Duration = 0, NoStore = true)]
        [HttpGet]
        [Route("status/{id}")]
        public IActionResult Status(QuestionnaireRevision id, Guid? translation, int? timezoneOffsetMinutes)
        {
            var pdfGenerationProgress = pdfService.Status(id, translation, DocumentType.Html); 
            if (pdfGenerationProgress == null)
                return this.Json(PdfStatus.Failed(PdfMessages.NotFound));

            long sizeInKb = this.GetFileSizeInKb(pdfGenerationProgress.FilePath);
            if (sizeInKb == 0)
                return pdfGenerationProgress.Status == PdfGenerationStatus.Finished 
                    ? this.Json(PdfStatus.Failed(PdfMessages.FailedToGenerate))
                    : this.Json(PdfStatus.InProgress(PdfMessages.PreparingToGenerate));

            return this.Json(
                pdfGenerationProgress.Status == PdfGenerationStatus.Finished
                    ? PdfStatus.Ready(
                        pdfGenerationProgress.TimeSinceFinished.TotalMinutes < 1
                            ? string.Format(PdfMessages.GenerateLessMinute, sizeInKb)
                            : string.Format(PdfMessages.Generate, (int)pdfGenerationProgress.TimeSinceFinished.TotalMinutes, sizeInKb))
                    : PdfStatus.InProgress(string.Format(PdfMessages.GeneratingSuccess, sizeInKb)));
        }
        
        [ResponseCache(Duration = 0, NoStore = true)]
        [Route("downloadPdf/{id}")]
        public IActionResult DownloadPdf(QuestionnaireRevision id, Guid? translation)
        {
            byte[]? content = pdfService.Download(id, translation, DocumentType.Pdf); 

            if (content != null)
            {
                var questionnaireTitle = this.pdfFactory.LoadQuestionnaireTitle(id.QuestionnaireId);
                // MS edge brakes on long file name
                string? validTitle = (questionnaireTitle?.Length < 250 ? questionnaireTitle : questionnaireTitle?.Substring(0, 250) ?? "questionnaire");
                return this.File(content, "application/pdf", $"{validTitle}.pdf");
            }

            return StatusCode((int)HttpStatusCode.NotFound);
        }
        
        [ResponseCache (Duration = 0, NoStore = true)]
        [HttpPost]
        [Route("generatePdf/{id}")]
        public async Task<IActionResult> GeneratePdf(QuestionnaireRevision id, Guid? translation, int? timezoneOffsetMinutes)
        {
            try
            {
                var pdfGenerationProgress = await pdfService.Enqueue(id, translation, DocumentType.Pdf, timezoneOffsetMinutes);

                if (pdfGenerationProgress.Status == PdfGenerationStatus.Failed)
                {
                    if (timezoneOffsetMinutes != null)
                    {
                        return this.Json(PdfStatus.Failed(PdfMessages.FailedToGenerate));
                    }
                    else
                    {
                        return this.Json(PdfStatus.InProgress(PdfMessages.PreparingToGenerate));
                    }
                }

                long sizeInKb = this.GetFileSizeInKb(pdfGenerationProgress.FilePath);

                if (sizeInKb == 0)
                    return pdfGenerationProgress.Status == PdfGenerationStatus.Finished 
                        ? this.Json(PdfStatus.Failed(PdfMessages.FailedToGenerate))
                        : this.Json(PdfStatus.InProgress(PdfMessages.PreparingToGenerate));

                return this.Json(
                    pdfGenerationProgress.Status == PdfGenerationStatus.Finished
                        ? PdfStatus.Ready(
                            pdfGenerationProgress.TimeSinceFinished.TotalMinutes < 1
                                ? string.Format(PdfMessages.GenerateLessMinute, sizeInKb)
                                : string.Format(PdfMessages.Generate, (int)pdfGenerationProgress.TimeSinceFinished.TotalMinutes, sizeInKb))
                        : PdfStatus.InProgress(string.Format(PdfMessages.GeneratingSuccess, sizeInKb)));
            }
            catch (PdfLimitReachedException ex)
            {
                return this.Json(PdfStatus.Failed(string.Format(PdfMessages.PdfLimitReached, ex.UserLimit)));
            }
        }

        [ResponseCache (Duration = 0, NoStore = true)]
        [HttpGet]
        [Route("statusPdf/{id}")]
        public IActionResult StatusPdf(QuestionnaireRevision id, Guid? translation, int? timezoneOffsetMinutes)
        {
            var pdfGenerationProgress = pdfService.Status(id, translation, DocumentType.Pdf);
            if (pdfGenerationProgress == null)
                return this.Json(PdfStatus.Failed(PdfMessages.NotFound));

            if (pdfGenerationProgress.Status == PdfGenerationStatus.Failed)
                return this.Json(PdfStatus.Failed(PdfMessages.FailedToGenerate));

            long sizeInKb = this.GetFileSizeInKb(pdfGenerationProgress.FilePath);

            if (sizeInKb == 0)
                return pdfGenerationProgress.Status == PdfGenerationStatus.Finished 
                    ? this.Json(PdfStatus.Failed(PdfMessages.FailedToGenerate))
                    : this.Json(PdfStatus.InProgress(PdfMessages.PreparingToGenerate));
            
            return this.Json(
                pdfGenerationProgress.Status == PdfGenerationStatus.Finished
                    ? PdfStatus.Ready(
                        pdfGenerationProgress.TimeSinceFinished.TotalMinutes < 1
                            ? string.Format(PdfMessages.GenerateLessMinute, sizeInKb)
                            : string.Format(PdfMessages.Generate, (int)pdfGenerationProgress.TimeSinceFinished.TotalMinutes, sizeInKb))
                    : PdfStatus.InProgress(string.Format(PdfMessages.GeneratingSuccess, sizeInKb)));
        }
        
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route("retry/{id}")]
        public async Task<ActionResult> Retry(QuestionnaireRevision? id, [FromBody]RetryRequest? retryRequest)
        {
            try
            {
                if (id == null || retryRequest == null)
                {
                    return StatusCode((int)HttpStatusCode.NotFound);
                }

                var pdfGenerationProgress = await pdfService.Retry(id, retryRequest.Translation, DocumentType.Pdf);
                return this.Json(PdfStatus.InProgress(PdfMessages.Retry));
            }
            catch (PdfLimitReachedException ex)
            {
                return this.Json(PdfStatus.Failed(string.Format(PdfMessages.PdfLimitReached, ex.UserLimit)));
            }
        }
            
        public class RetryRequest
        {
            public Guid? Translation { set; get; }
        }
        
        
        private static bool installed = false;
        private static readonly Lock lockInstalled = new Lock();
        
        [Authorize(Roles = "Administrator")]
        [ResponseCache(Duration = 0, NoStore = true)]
        [HttpGet]
        [Route("install")]
        public JsonResult Install()
        {
            try
            {
                lock (lockInstalled)
                {
                    if (!installed)
                    {
                        var returnCode = Microsoft.Playwright.Program.Main(["install"]);
                        installed = true;
                        return this.Json("Ok. Installed" + returnCode);
                    }
                }

                return this.Json("Ok");
            }
            catch (Exception e)
            {
                installed = false;
                return this.Json("Fail: " + e.Message);
            }
        }
        
        [Authorize(Roles = "Administrator")]
        [ResponseCache(Duration = 0, NoStore = true)]
        [HttpGet]
        [Route("queryInfo")]
        public IActionResult QueryInfo()
        {
            var json = pdfService.GetCurrentInfoJson();
            return Ok(json);
        }
        
        private long GetFileSizeInKb(string filepath)
            => this.fileSystemAccessor.IsFileExists(filepath)
                ? this.fileSystemAccessor.GetFileSize(filepath) / 1024
                : 0;
    }
}
