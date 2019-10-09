using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Configuration;
using System.Globalization;
using System.IO;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Internal;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Shark.PdfConvert;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.Pdf;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.Infrastructure.FileSystem;
using WB.UI.Designer.Code.Attributes;
using WB.UI.Designer.Extensions;
using WB.UI.Designer.Resources;

namespace WB.UI.Designer.Areas.Pdf.Controllers
{
    [Area("Pdf")]
    [Route("pdf")]
    public class PdfController : Controller
    {
        #region Subclasses

        private class PdfGenerationProgress
        {
            private DateTime? finishTime;

            public string FilePath { get; } = Path.GetTempFileName();
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
        private readonly PdfSettings pdfSettings;
        private readonly ILogger logger;
        private readonly IFileSystemAccessor fileSystemAccessor;
        private readonly IRazorViewEngine razorViewEngine;
        private readonly IServiceProvider serviceProvider;
        private readonly ITempDataProvider tempDataProvider;

        public PdfController(
            IPdfFactory pdfFactory, 
            ILogger<PdfController> logger,
            IFileSystemAccessor fileSystemAccessor,
            IRazorViewEngine razorViewEngine,
            IServiceProvider serviceProvider,
            ITempDataProvider tempDataProvider,
            IOptions<PdfSettings> pdfOptions)
        {
            this.pdfFactory = pdfFactory;
            this.pdfSettings = pdfOptions.Value;
            this.logger = logger;
            this.fileSystemAccessor = fileSystemAccessor;
            this.razorViewEngine = razorViewEngine;
            this.serviceProvider = serviceProvider;
            this.tempDataProvider = tempDataProvider;
        }

        protected IActionResult RenderQuestionnaire(Guid id, Guid requestedByUserId, string requestedByUserName, Guid? translation, string cultureCode, int timezoneOffsetMinutes)
        {
            if (!string.IsNullOrWhiteSpace(cultureCode))
            {
                CultureInfo culture = CultureInfo.GetCultureInfo(cultureCode);
                Thread.CurrentThread.CurrentCulture = culture;
                Thread.CurrentThread.CurrentUICulture = culture;
            }
            PdfQuestionnaireModel questionnaire = this.LoadQuestionnaire(id, requestedByUserId, requestedByUserName, translation, false);
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
        public IActionResult PrintPreview(Guid id, Guid? translation)
        {
            PdfQuestionnaireModel questionnaire = this.LoadQuestionnaire(id, User.GetId(), User.GetUserName(), translation: translation, useDefaultTranslation: true);
            if (questionnaire == null)
            {
                return StatusCode((int)HttpStatusCode.NotFound);
            }
            return this.View("RenderQuestionnaire", questionnaire);
        }

        [ResponseCache(Duration = 0, NoStore = true)]
        [Route("download/{id}")]
        public IActionResult Download(Guid id, Guid? translation)
        {
            var pdfKey = id.ToString() + translation;
            PdfGenerationProgress pdfGenerationProgress = GeneratedPdfs.GetOrNull(pdfKey);

            if (pdfGenerationProgress?.IsFinished == true)
            {
                var questionnaireTitle = this.pdfFactory.LoadQuestionnaireTitle(id);

                byte[] content = this.fileSystemAccessor.ReadAllBytes(pdfGenerationProgress.FilePath);

                this.fileSystemAccessor.DeleteFile(pdfGenerationProgress.FilePath);
                GeneratedPdfs.TryRemove(pdfKey, out _);

                // MS edge brakes on long file name
                string validTitle = questionnaireTitle.Length < 250 ? questionnaireTitle : questionnaireTitle.Substring(0, 250);

                return this.File(content, "application/pdf", $"{validTitle}.pdf");
            }
            else
            {
                return StatusCode((int)HttpStatusCode.NotFound);
            }
        }

        [ResponseCache (Duration = 0, NoStore = true)]
        [HttpGet]
        [Route("status/{id}")]
        public JsonResult Status(Guid id, Guid? translation, int? timezoneOffsetMinutes)
        {
            var pdfKey = id.ToString() + translation;
            PdfGenerationProgress pdfGenerationProgress = GeneratedPdfs.GetOrAdd(pdfKey, _ => StartNewPdfGeneration(id, translation, timezoneOffsetMinutes));

            if (pdfGenerationProgress.IsFailed)
                return this.Json(PdfStatus.Failed(PdfMessages.FailedToGenerate));

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

        [HttpPost]
        [Route("retry/{id}")]
        public ActionResult Retry(Guid id, Guid? translation, int? timezoneOffsetMinutes)
        {
            var pdfKey = id.ToString() + translation;
            var cultureCode = CultureInfo.CurrentUICulture.Name;
            PdfGenerationProgress pdfGenerationProgress = GeneratedPdfs.GetOrAdd(pdfKey, _ => StartNewPdfGeneration(id, translation, timezoneOffsetMinutes));
            if (pdfGenerationProgress != null && pdfGenerationProgress.IsFailed)
            {
                GeneratedPdfs.TryRemove(pdfKey, out _);
            }
            GeneratedPdfs.GetOrAdd(pdfKey, _ => StartNewPdfGeneration(id, translation, timezoneOffsetMinutes));
            return this.Json(PdfStatus.InProgress(PdfMessages.Retry));
        }

        private PdfGenerationProgress StartNewPdfGeneration(Guid id, Guid? translation, int? timezoneOffsetMinutes)
        {
            var newPdfGenerationProgress = new PdfGenerationProgress();
            this.StartRenderPdf(id, newPdfGenerationProgress, translation, timezoneOffsetMinutes ?? 0);
            return newPdfGenerationProgress;
        }
        
        private void StartRenderPdf(Guid id, PdfGenerationProgress generationProgress, Guid? translation, int timezoneOffsetMinutes)
        {
            var pathToWkHtmlToPdfExecutable = this.GetPathToWKHtmlToPdfExecutableOrThrow();

            PdfQuestionnaireModel questionnaire = this.LoadQuestionnaire(id, User.GetId(), User.GetUserName(), translation, false);
            if (questionnaire == null)
            {
                throw new ArgumentException();
            }
            questionnaire.TimezoneOffsetMinutes = timezoneOffsetMinutes;

            var questionnaireHtml = RenderActionResultToString(nameof(RenderQuestionnaire), questionnaire).Result;

            ControllerContext.RouteData.Routers.Add(AttributeRouting.CreateAttributeMegaRoute(serviceProvider));
            var pageFooterUrl = new UrlHelper(ControllerContext).Link("QuestionnaireFooter", new { });

            Task.Factory.StartNew(() =>
            {
                try
                {
                    PdfConvert.Convert(new PdfConversionSettings
                    {
                        Content = questionnaireHtml,
                        PageFooterUrl = pageFooterUrl,
                        OutputPath = generationProgress.FilePath,
                        WkHtmlToPdfExeName = pathToWkHtmlToPdfExecutable,
                        ExecutionTimeout = this.pdfSettings.PdfGenerationTimeoutInMilliseconds,
                        TempFilesPath = Path.GetTempPath(),
                        Size = PdfPageSize.A4,
                        Margins = new PdfPageMargins() {Top = 10, Bottom = 7, Left = 0, Right = 0},
                    });
                    Thread.Sleep(10000);
                    generationProgress.Finish();
                }
                catch (Exception exception)
                {
                    this.logger.LogError(exception, $"Failed to generate PDF {id.FormatGuid()}");
                    generationProgress.Fail();
                }
            }, TaskCreationOptions.LongRunning);
        }

        private async Task<string> RenderActionResultToString(string viewName, object model)
        {
            var httpContext = new DefaultHttpContext
            {
                RequestServices = serviceProvider, 
                Request =
                {
                    Host = this.Request.Host,
                    IsHttps = this.Request.IsHttps,
                    Scheme = this.Request.Scheme,
                },
            };

            
            var routeData = new RouteData();
            routeData.Values.Add("area", "Pdf");
            routeData.Values.Add("controller", "Pdf");
            routeData.Routers.Add(AttributeRouting.CreateAttributeMegaRoute(serviceProvider));
            
            var actionContext = new ActionContext(httpContext, routeData, new ActionDescriptor() { RouteValues = new Dictionary<string, string>{{"area", "Pdf"}}});

            using (var sw = new StringWriter())
            {
                var viewResult = razorViewEngine.FindView(actionContext, viewName, false);
                if (viewResult.View == null)
                {
                    throw new ArgumentNullException($"{viewName} does not match any available view");
                }

                var viewDictionary = new ViewDataDictionary(new EmptyModelMetadataProvider(), new ModelStateDictionary())
                {
                    Model = model
                };

                var viewContext = new ViewContext(
                    actionContext,
                    viewResult.View,
                    viewDictionary,
                    new TempDataDictionary(actionContext.HttpContext, tempDataProvider),
                    sw,
                    new HtmlHelperOptions()
                );

                await viewResult.View.RenderAsync(viewContext);

                return sw.ToString();
            }
        }

        private string GetPathToWKHtmlToPdfExecutableOrThrow()
        {
            string path = Path.GetFullPath(pdfSettings.WKHtmlToPdfExecutablePath);

            if (!System.IO.File.Exists(path))
                throw new ConfigurationErrorsException(string.Format("Path to wkhtmltopdf.exe is incorrect ({0}). Please install wkhtmltopdf.exe and/or update server configuration.", path));

            return path;
        }

        private PdfQuestionnaireModel LoadQuestionnaire(Guid id, Guid requestedByUserId, string requestedByUserName, Guid? translation, bool useDefaultTranslation)
        {
            return this.pdfFactory.Load(id.FormatGuid(), requestedByUserId, requestedByUserName, translation, useDefaultTranslation);
        }

        private long GetFileSizeInKb(string filepath)
            => this.fileSystemAccessor.IsFileExists(filepath)
                ? this.fileSystemAccessor.GetFileSize(filepath) / 1024
                : 0;
    }
}
