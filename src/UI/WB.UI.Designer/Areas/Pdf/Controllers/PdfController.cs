using System;
using System.Collections.Concurrent;
using System.Globalization;
using System.IO;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.Pdf;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.Infrastructure.FileSystem;
using WB.UI.Designer.Resources;
using WB.Core.BoundedContexts.Designer;
using PuppeteerSharp;
using System.Linq;
using PuppeteerSharp.Media;

namespace WB.UI.Designer.Areas.Pdf.Controllers
{
    [Area("Pdf")]
    [Route("pdf")]
    [Authorize]
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
        private readonly ILogger logger;
        private readonly IFileSystemAccessor fileSystemAccessor;

        public PdfController(
            IPdfFactory pdfFactory, 
            ILogger<PdfController> logger,
            IFileSystemAccessor fileSystemAccessor)
        {
            this.pdfFactory = pdfFactory;
            this.logger = logger;
            this.fileSystemAccessor = fileSystemAccessor;            
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
        
        [Route("printpreview/{id}", Name = "PrintPreview")]
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

        static SemaphoreSlim chromeDownloader = new SemaphoreSlim(1);

        private void StartRenderPdf(Guid id, PdfGenerationProgress generationProgress, Guid? translation, int timezoneOffsetMinutes)
        {
            PdfQuestionnaireModel questionnaire = this.LoadQuestionnaire(id, User.GetId(), User.GetUserName(), translation, false);
            if (questionnaire == null)
            {
                throw new ArgumentException();
            }
            questionnaire.TimezoneOffsetMinutes = timezoneOffsetMinutes;

            var link = this.Url.RouteUrl("PrintPreview", new { id, translation }, this.Request.Scheme, this.Request.Host.ToString());
            var cookies = this.Request.Cookies.ToList();
            var cookieDomain = this.Request.Host.ToString();

            Task.Factory.StartNew(async () =>
            {
                try
                {
                    try
                    {
                        await chromeDownloader.WaitAsync();
                        await new BrowserFetcher().DownloadAsync(BrowserFetcher.DefaultRevision);
                    }
                    finally
                    {
                        chromeDownloader.Release();
                    }
                                       

                    async Task<Browser> GetBrowser()
                    {
                        var remote = Environment.GetEnvironmentVariable("PUPPETEER_REMOTE_ADDR");
                        if (remote != null)
                        {
                            return await Puppeteer.ConnectAsync(new ConnectOptions
                            {
                                BrowserWSEndpoint = remote
                            });
                        } else
                        {
                            var launchArgs = Environment.GetEnvironmentVariable("PUPPETEER_LAUNCH_ARGS");
                            return await Puppeteer.LaunchAsync(new LaunchOptions
                            {
                                Headless = true,
                                Args = launchArgs == null ? Array.Empty<string>() : launchArgs.Split(' ')
                            });
                        }
                    }

                    using var browser = await GetBrowser();
                    
                    var page = await browser.NewPageAsync();
                                        
                    foreach(var cookie in cookies)
                    {
                        await page.SetCookieAsync(new CookieParam
                        {
                            Name = cookie.Key,
                            Value = cookie.Value,
                            Domain = cookieDomain
                        });
                    }
                  
                    await page.GoToAsync(link);
                    await page.PdfAsync(generationProgress.FilePath, new PdfOptions
                    {
                       DisplayHeaderFooter = true,
                       Format = PaperFormat.A4,
                       MarginOptions = new MarginOptions
                       {
                           Top = "10px", Bottom = "15px", Left = "20px", Right = "20px"
                       },
                       HeaderTemplate = "",
                       PrintBackground = true,
                       Scale = 1,
                       FooterTemplate = @"<table style='width: 100%; margin-right: 20px;font-size: 10px; text-align: right'><td><span class='pageNumber'></span> / <span class='totalPages'></span></td></table>"
                    });

                    generationProgress.Finish();
                }
                catch (Exception exception)
                {
                    this.logger.LogError(exception, $"Failed to generate PDF {id.FormatGuid()}");
                    generationProgress.Fail();
                }
            }, TaskCreationOptions.LongRunning);
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
