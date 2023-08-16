﻿using System;
using System.Collections.Concurrent;
using System.Globalization;
using System.IO;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Shark.PdfConvert;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.Pdf;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.Infrastructure.FileSystem;
using WB.UI.Designer.Resources;
using WB.Core.BoundedContexts.Designer;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.ChangeHistory;
using WB.UI.Designer.Controllers.Api.Designer;
using WB.UI.Shared.Web.Services;

namespace WB.UI.Designer.Areas.Pdf.Controllers
{
    [Area("Pdf")]
    [Route("pdf")]
    [AuthorizeOrAnonymousQuestionnaire]
    [QuestionnairePermissions]
    public class PdfController : Controller
    {
        #region Subclasses

        private class PdfGenerationProgress
        {
            public PdfGenerationProgress()
            {
                FilePath = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
            }

            private DateTime? finishTime;

            public string FilePath { get; }
            public bool IsFailed { get; private set; }
            public bool IsFinished => finishTime.HasValue;
            public TimeSpan TimeSinceFinished => this.finishTime.HasValue ? DateTime.Now - this.finishTime.Value : TimeSpan.Zero;

            public void Fail() => this.IsFailed = true;
            public void Finish() => this.finishTime = DateTime.Now;
        }

        public class PdfStatus
        {
            private PdfStatus(string message, bool readyForDownload = false, bool canRetry = false)
            {
                this.Message = message;
                this.ReadyForDownload = readyForDownload;
                this.CanRetry = canRetry;
            }

            public string Message { get; }
            public bool ReadyForDownload { get; }
            public bool CanRetry { get; }

            public static PdfStatus InProgress(string message) => new PdfStatus(message);
            public static PdfStatus Failed(string message) => new PdfStatus(message, canRetry: true);
            public static PdfStatus Ready(string message) => new PdfStatus(message, readyForDownload: true);
        }

        #endregion

        private static readonly ConcurrentDictionary<string, PdfGenerationProgress> GeneratedPdfs = new ConcurrentDictionary<string, PdfGenerationProgress>();

        private readonly IPdfFactory pdfFactory;
        private readonly IOptions<PdfSettings> pdfSettings;
        private readonly ILogger logger;
        private readonly IFileSystemAccessor fileSystemAccessor;
        private readonly IViewRenderService viewRenderingService;

        public PdfController(
            IPdfFactory pdfFactory, 
            ILogger<PdfController> logger,
            IFileSystemAccessor fileSystemAccessor,
            IOptions<PdfSettings> pdfOptions, 
            IViewRenderService viewRenderingService)
        {
            this.pdfFactory = pdfFactory;
            this.pdfSettings = pdfOptions;
            this.viewRenderingService = viewRenderingService;
            this.logger = logger;
            this.fileSystemAccessor = fileSystemAccessor;
        }

        protected IActionResult RenderQuestionnaire(QuestionnaireRevision id, Guid? requestedByUserId, string? requestedByUserName, Guid? translation, string cultureCode, int timezoneOffsetMinutes)
        {
            if (!string.IsNullOrWhiteSpace(cultureCode))
            {
                CultureInfo culture = CultureInfo.GetCultureInfo(cultureCode);
                Thread.CurrentThread.CurrentCulture = culture;
                Thread.CurrentThread.CurrentUICulture = culture;
            }
            PdfQuestionnaireModel? questionnaire = this.LoadQuestionnaire(id, requestedByUserId, requestedByUserName, translation, false);
            if (questionnaire == null)
            {
                return StatusCode((int)HttpStatusCode.NotFound);
            }
            questionnaire.TimezoneOffsetMinutes = timezoneOffsetMinutes;
            return this.View("RenderQuestionnaire", questionnaire);
        }

        [AllowAnonymous]
        [HttpGet]
        [Route("questionnairefooter", Name = "QuestionnaireFooter")]
        public ActionResult RenderQuestionnaireFooter()
        {
            return this.View("RenderQuestionnaireFooter");
        }

        [Route("printpreview/{id}")]
        public IActionResult PrintPreview(QuestionnaireRevision id, Guid? translation)
        {
            PdfQuestionnaireModel? questionnaire = this.LoadQuestionnaire(id, User.GetIdOrNull(), User.GetUserNameOrNull(), translation: translation, useDefaultTranslation: true);
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
            var pdfKey = GetHtmlKey(id, translation);
            PdfGenerationProgress pdfGenerationProgress = GeneratedPdfs.GetOrNull(pdfKey);

            if (pdfGenerationProgress?.IsFinished == true)
            {
                var questionnaireTitle = this.pdfFactory.LoadQuestionnaireTitle(id.QuestionnaireId);

                byte[] content = this.fileSystemAccessor.ReadAllBytes(pdfGenerationProgress.FilePath);

                this.fileSystemAccessor.DeleteFile(pdfGenerationProgress.FilePath);
                GeneratedPdfs.TryRemove(pdfKey, out _);

                // MS edge brakes on long file name
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
        public JsonResult Status(QuestionnaireRevision id, Guid? translation, int? timezoneOffsetMinutes)
        {
            var pdfKey = GetHtmlKey(id, translation);
            PdfGenerationProgress pdfGenerationProgress = GeneratedPdfs.GetOrAdd(pdfKey, _ => StartNewHtmlGeneration(id, translation, timezoneOffsetMinutes));

            long sizeInKb = this.GetFileSizeInKb(pdfGenerationProgress.FilePath);
            if (sizeInKb == 0)
                return pdfGenerationProgress.IsFinished 
                    ? this.Json(PdfStatus.Failed(PdfMessages.FailedToGenerate))
                    : this.Json(PdfStatus.InProgress(PdfMessages.PreparingToGenerate));

            return this.Json(
                pdfGenerationProgress.IsFinished
                    ? PdfStatus.Ready(
                        pdfGenerationProgress.TimeSinceFinished.TotalMinutes < 1
                            ? string.Format(PdfMessages.GenerateLessMinute, sizeInKb)
                            : string.Format(PdfMessages.Generate, (int)pdfGenerationProgress.TimeSinceFinished.TotalMinutes, sizeInKb))
                    : PdfStatus.InProgress(string.Format(PdfMessages.GeneratingSuccess, sizeInKb)));
        }
        
        //

        [ResponseCache(Duration = 0, NoStore = true)]
        [Route("downloadPdf/{id}")]
        public IActionResult DownloadPdf(QuestionnaireRevision id, Guid? translation)
        {
            var pdfKey = GetPdfKey(id, translation);
            PdfGenerationProgress pdfGenerationProgress = GeneratedPdfs.GetOrNull(pdfKey);

            if (pdfGenerationProgress?.IsFinished == true)
            {
                var questionnaireTitle = this.pdfFactory.LoadQuestionnaireTitle(id.QuestionnaireId);

                byte[] content = this.fileSystemAccessor.ReadAllBytes(pdfGenerationProgress.FilePath);

                this.fileSystemAccessor.DeleteFile(pdfGenerationProgress.FilePath);
                GeneratedPdfs.TryRemove(pdfKey, out _);

                // MS edge brakes on long file name
                string? validTitle = (questionnaireTitle?.Length < 250 ? questionnaireTitle : questionnaireTitle?.Substring(0, 250) ?? "questionnaire");

                return this.File(content, "application/pdf", $"{validTitle}.pdf");
            }
            else
            {
                return StatusCode((int)HttpStatusCode.NotFound);
            }
        }

        [ResponseCache (Duration = 0, NoStore = true)]
        [HttpGet]
        [Route("statusPdf/{id}")]
        public JsonResult StatusPdf(QuestionnaireRevision id, Guid? translation, int? timezoneOffsetMinutes)
        {
            var pdfKey = GetPdfKey(id, translation);
            PdfGenerationProgress pdfGenerationProgress = GeneratedPdfs.GetOrAdd(pdfKey, _ => StartNewPdfGeneration(id, translation, timezoneOffsetMinutes));

            if (pdfGenerationProgress.IsFailed)
            {
                if (timezoneOffsetMinutes != null)
                {
                    return this.Json(PdfStatus.Failed(PdfMessages.FailedToGenerate));
                }
                else
                {
                    GeneratedPdfs.TryRemove(pdfKey, out _);
                    GeneratedPdfs.GetOrAdd(pdfKey, _ => StartNewPdfGeneration(id, translation, timezoneOffsetMinutes));
                    return this.Json(PdfStatus.InProgress(PdfMessages.PreparingToGenerate));
                }
            }

            long sizeInKb = this.GetFileSizeInKb(pdfGenerationProgress.FilePath);

            if (sizeInKb == 0)
                return pdfGenerationProgress.IsFinished 
                    ? this.Json(PdfStatus.Failed(PdfMessages.FailedToGenerate))
                    : this.Json(PdfStatus.InProgress(PdfMessages.PreparingToGenerate));

            return this.Json(
                pdfGenerationProgress.IsFinished
                    ? PdfStatus.Ready(
                        pdfGenerationProgress.TimeSinceFinished.TotalMinutes < 1
                            ? string.Format(PdfMessages.GenerateLessMinute, sizeInKb)
                            : string.Format(PdfMessages.Generate, (int)pdfGenerationProgress.TimeSinceFinished.TotalMinutes, sizeInKb))
                    : PdfStatus.InProgress(string.Format(PdfMessages.GeneratingSuccess, sizeInKb)));
        }

        private static string GetPdfKey(QuestionnaireRevision id, Guid? translation)
        {
            return id.ToString() + ":" + translation;
        }
        
        private static string GetHtmlKey(QuestionnaireRevision id, Guid? translation)
        {
            return "html:" + id.ToString() + ":" + translation;
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route("retry/{id}")]
        public ActionResult Retry(QuestionnaireRevision? id, [FromBody]RetryRequest retryRequest)
        {
            if (id == null || retryRequest == null)
            {
                return StatusCode((int)HttpStatusCode.NotFound);
            }

            var pdfKey = GetPdfKey(id, retryRequest.Translation);
            PdfGenerationProgress pdfGenerationProgress = 
                GeneratedPdfs.GetOrAdd(pdfKey, _ => StartNewPdfGeneration(id, retryRequest.Translation, null));
            if (pdfGenerationProgress != null && pdfGenerationProgress.IsFailed)
            {
                GeneratedPdfs.TryRemove(pdfKey, out _);
            }
            
            GeneratedPdfs.GetOrAdd(pdfKey, _ => StartNewPdfGeneration(id, retryRequest.Translation, null));
            return this.Json(PdfStatus.InProgress(PdfMessages.Retry));
        }
            
        public class RetryRequest
        {
            public Guid? Translation { set; get; }
        }

        private PdfGenerationProgress StartNewHtmlGeneration(QuestionnaireRevision id, Guid? translation, int? timezoneOffsetMinutes)
        {
            var newPdfGenerationProgress = new PdfGenerationProgress();
            
            var questionnaireHtml = GetHtmlContent(id, newPdfGenerationProgress, translation, timezoneOffsetMinutes ?? 0);
            if (!newPdfGenerationProgress.IsFailed)
            {
                System.IO.File.WriteAllText(newPdfGenerationProgress.FilePath, questionnaireHtml);
                newPdfGenerationProgress.Finish();
            }
            
            return newPdfGenerationProgress;
        }
        
        private PdfGenerationProgress StartNewPdfGeneration(QuestionnaireRevision id, Guid? translation, int? timezoneOffsetMinutes)
        {
            var newPdfGenerationProgress = new PdfGenerationProgress();
            this.StartRenderPdf(id, newPdfGenerationProgress, translation, timezoneOffsetMinutes ?? 0);
            return newPdfGenerationProgress;
        }

        private string GetHtmlContent(QuestionnaireRevision id, PdfGenerationProgress generationProgress, Guid? translation, int timezoneOffsetMinutes)
        {
            PdfQuestionnaireModel? questionnaire = this.LoadQuestionnaire(id, User.GetIdOrNull(), User.GetUserNameOrNull(), translation, false);
            if (questionnaire == null)
            {
                generationProgress.Fail();
                return string.Empty;
            }
            questionnaire.TimezoneOffsetMinutes = timezoneOffsetMinutes;

            return RenderActionResultToString(nameof(RenderQuestionnaire), questionnaire).Result;
        }

        private void StartRenderPdf(QuestionnaireRevision id, PdfGenerationProgress generationProgress, Guid? translation, int timezoneOffsetMinutes)
        {
            var questionnaireHtml = GetHtmlContent(id, generationProgress, translation, timezoneOffsetMinutes);
            if(generationProgress.IsFailed)
                return;

            var pageFooterUrl = Url.Link("QuestionnaireFooter", new { });
            var pathToWkHtmlToPdfExecutable = this.pdfSettings.Value.WKHtmlToPdfExecutablePath;
            Task.Factory.StartNew(() =>
            {
                try
                {
                    PdfConvert.Convert(new PdfConversionSettings
                    {
                        Content = questionnaireHtml,
                        PageFooterUrl = pageFooterUrl,
                        OutputPath = generationProgress.FilePath,
                         
                        PdfToolPath = pathToWkHtmlToPdfExecutable, 
                        WkHtmlToPdfExeName = this.pdfSettings.Value.WkHtmlToPdfExeName,
                        ExecutionTimeout = this.pdfSettings.Value.PdfGenerationTimeoutInMilliseconds,
                        TempFilesPath = Path.GetTempPath(),
                        Size = PdfPageSize.A4,
                        Margins = new PdfPageMargins() {Top = 10, Bottom = 7, Left = 0, Right = 0},
                    });

                    generationProgress.Finish();
                }
                catch (Exception exception)
                {
                    this.logger.LogError(exception, $"Failed to generate PDF {id}");
                    generationProgress.Fail();
                }
            }, TaskCreationOptions.LongRunning);
        }

        private async Task<string> RenderActionResultToString(string viewName, object model)
        {
            var uri = new Uri($"{this.Request.Scheme}://{this.Request.Host}{this.Request.PathBase}");
            string webRoot = uri.ToString().TrimEnd('/');
            string webAppRoot = uri.GetLeftPart(System.UriPartial.Authority);
            var routeData = new Microsoft.AspNetCore.Routing.RouteData();
            routeData.DataTokens.Add("area", "Pdf");
            routeData.Values.Add("controller", "Pdf");
            routeData.Values.Add("area", "Pdf");
            return await this.viewRenderingService.RenderToStringAsync(viewName, model, webRoot, webAppRoot, routeData);
        }

        private PdfQuestionnaireModel? LoadQuestionnaire(QuestionnaireRevision id, Guid? requestedByUserId, string? requestedByUserName, Guid? translation, bool useDefaultTranslation)
        {
            return this.pdfFactory.Load(id, requestedByUserId, requestedByUserName, translation, useDefaultTranslation);
        }

        private long GetFileSizeInKb(string filepath)
            => this.fileSystemAccessor.IsFileExists(filepath)
                ? this.fileSystemAccessor.GetFileSize(filepath) / 1024
                : 0;
    }
}
