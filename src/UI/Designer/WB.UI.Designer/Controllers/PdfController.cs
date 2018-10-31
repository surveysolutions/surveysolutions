using System;
using System.Collections.Concurrent;
using System.Configuration;
using System.Globalization;
using System.IO;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;
using Shark.PdfConvert;
using WB.Core.BoundedContexts.Designer.Implementation.Services.Accounts.Membership;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.Pdf;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.GenericSubdomains.Portable.ServiceLocation;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure.FileSystem;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.Infrastructure.Transactions;
using WB.UI.Designer.Pdf;
using WB.UI.Designer.Resources;
using WB.UI.Shared.Web.Filters;
using PdfConvert = Shark.PdfConvert.PdfConvert;

namespace WB.UI.Designer.Controllers
{
    public class PdfController : BaseController
    {
        #region Subclasses

        private class PdfGenerationProgress
        {
            private DateTime? finishTime = null;

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

        public PdfController(
            IMembershipUserService userHelper, 
            IPdfFactory pdfFactory, 
            PdfSettings pdfSettings,
            ILogger logger,
            IFileSystemAccessor fileSystemAccessor)
            : base(userHelper)
        {
            this.pdfFactory = pdfFactory;
            this.pdfSettings = pdfSettings;
            this.logger = logger;
            this.fileSystemAccessor = fileSystemAccessor;
        }

        [LocalOrDevelopmentAccessOnly]
        [System.Web.Mvc.AllowAnonymous]
        public ActionResult RenderQuestionnaire(Guid id, Guid requestedByUserId, string requestedByUserName, Guid? translation, string cultureCode)
        {
            if (!string.IsNullOrWhiteSpace(cultureCode))
            {
                CultureInfo culture = CultureInfo.GetCultureInfo(cultureCode);
                Thread.CurrentThread.CurrentCulture = culture;
                Thread.CurrentThread.CurrentUICulture = culture;
            }
            var questionnaire = this.LoadQuestionnaireOrThrow404(id, requestedByUserId, requestedByUserName, translation, false);
            return this.View("RenderQuestionnaire", questionnaire);
        }

        [System.Web.Mvc.AllowAnonymous]
        public ActionResult RenderQuestionnaireFooter()
        {
            return this.View("RenderQuestionnaireFooter");
        }

        public ActionResult PrintPreview(Guid id, Guid? translation)
        {
            PdfQuestionnaireModel questionnaire = this.LoadQuestionnaireOrThrow404(id, UserHelper.WebUser.UserId, UserHelper.WebUser.UserName, translation: translation, useDefaultTranslation: true);
            if (questionnaire == null)
            {
                throw new HttpResponseException(HttpStatusCode.NotFound);
            }
            return this.View("RenderQuestionnaire", questionnaire);
        }

        [OutputCache(VaryByParam = "*", Duration = 0, NoStore = true)]
        public ActionResult Download(Guid id, Guid? translation)
        {
            var pdfKey = id.ToString() + translation;
            PdfGenerationProgress pdfGenerationProgress = GeneratedPdfs.GetOrNull(pdfKey);

            if (pdfGenerationProgress?.IsFinished == true)
            {
                var questionnaireTitle = this.pdfFactory.LoadQuestionnaireTitle(id);

                byte[] content = this.fileSystemAccessor.ReadAllBytes(pdfGenerationProgress.FilePath);

                this.fileSystemAccessor.DeleteFile(pdfGenerationProgress.FilePath);
                GeneratedPdfs.TryRemove(pdfKey, out _);

                return this.File(content, "application/pdf", $"{questionnaireTitle}.pdf");
            }
            else
            {
                return this.HttpNotFound();
            }
        }

        [OutputCache(VaryByParam = "*", Duration = 0, NoStore = true)]
        [System.Web.Mvc.HttpGet]
        public JsonResult Status(Guid id, Guid? translation)
        {
            var pdfKey = id.ToString() + translation;
            var cultureCode = CultureInfo.CurrentUICulture.Name;
            PdfGenerationProgress pdfGenerationProgress = GeneratedPdfs.GetOrAdd(pdfKey, _ => StartNewPdfGeneration(id, translation, cultureCode));

            if (pdfGenerationProgress.IsFailed)
                return this.Json(PdfStatus.Failed(PdfMessages.FailedToGenerate), JsonRequestBehavior.AllowGet);

            long sizeInKb = this.GetFileSizeInKb(pdfGenerationProgress.FilePath);

            if (sizeInKb == 0)
                return pdfGenerationProgress.IsFinished 
                    ? this.Json(PdfStatus.Failed(PdfMessages.FailedToGenerate), JsonRequestBehavior.AllowGet)
                    : this.Json(PdfStatus.InProgress(PdfMessages.PreparingToGenerate), JsonRequestBehavior.AllowGet);

            return this.Json(
                pdfGenerationProgress.IsFinished
                    ? PdfStatus.Ready(
                        pdfGenerationProgress.TimeSinceFinished.TotalMinutes < 1
                            ? string.Format(PdfMessages.GenerateLessMinute, sizeInKb)
                            : string.Format(PdfMessages.Generate, (int)pdfGenerationProgress.TimeSinceFinished.TotalMinutes, sizeInKb))
                    : PdfStatus.InProgress(string.Format(PdfMessages.GeneratingSuccess, sizeInKb)),
                JsonRequestBehavior.AllowGet);
        }

        [System.Web.Mvc.HttpPost]
        public ActionResult Retry(Guid id, Guid? translation)
        {
            var pdfKey = id.ToString() + translation;
            var cultureCode = CultureInfo.CurrentUICulture.Name;
            PdfGenerationProgress pdfGenerationProgress = GeneratedPdfs.GetOrAdd(pdfKey, _ => StartNewPdfGeneration(id, translation, cultureCode));
            if (pdfGenerationProgress != null && pdfGenerationProgress.IsFailed)
            {
                GeneratedPdfs.TryRemove(pdfKey, out _);
            }
            GeneratedPdfs.GetOrAdd(pdfKey, _ => StartNewPdfGeneration(id, translation, cultureCode));
            return this.Json(PdfStatus.InProgress(PdfMessages.Retry));
        }

        private PdfGenerationProgress StartNewPdfGeneration(Guid id, Guid? translation, string cultureCode)
        {
            var newPdfGenerationProgress = new PdfGenerationProgress();
            this.StartRenderPdf(id, newPdfGenerationProgress, translation, cultureCode);
            return newPdfGenerationProgress;
        }
        
        private void StartRenderPdf(Guid id, PdfGenerationProgress generationProgress, Guid? translation, string cultureCode)
        {
            var pathToWkHtmlToPdfExecutable = this.GetPathToWKHtmlToPdfExecutableOrThrow();

            var renderQuestionnaireResult = RenderQuestionnaire(id: id,
                requestedByUserId: this.UserHelper.WebUser.UserId,
                requestedByUserName: this.UserHelper.WebUser.UserName,
                translation: translation,
                cultureCode: cultureCode);
            var questionnaireHtml = RenderActionResultToString(renderQuestionnaireResult);
            var pageFooterUrl = GlobalHelper.GenerateUrl(nameof(RenderQuestionnaireFooter), "Pdf", new { });

            Task.Factory.StartNew(() =>
            {
                try
                {
                    if (!string.IsNullOrWhiteSpace(cultureCode))
                    {
                        CultureInfo culture = CultureInfo.GetCultureInfo(cultureCode);
                        Thread.CurrentThread.CurrentCulture = culture;
                        Thread.CurrentThread.CurrentUICulture = culture;
                    }

                    PdfConvert.Convert(new PdfConversionSettings
                    {
                        Content = questionnaireHtml,
                        PageFooterUrl = pageFooterUrl,
                        OutputPath = generationProgress.FilePath,
                        WkHtmlToPdfExeName = pathToWkHtmlToPdfExecutable,
                        ExecutionTimeout = this.pdfSettings.PdfGenerationTimeoutInMilliseconds,
                        TempFilesPath = Path.GetTempPath(),
                        Size = PdfPageSize.A4,
                        Margins = new PdfPageMargins() { Top = 10, Bottom = 7, Left = 0, Right = 0 },
                    });

                    generationProgress.Finish();
                }
                catch (Exception exception)
                {
                    this.logger.Error($"Failed to generate PDF {id.FormatGuid()}", exception);
                    generationProgress.Fail();
                }
            }, TaskCreationOptions.LongRunning);
        }

        private string RenderActionResultToString(ActionResult result)
        {
            var sb = new StringBuilder();
            using (var memWriter = new StringWriter(sb))
            {
                var fakeResponse = new HttpResponse(memWriter);
                var fakeContext = new HttpContext(System.Web.HttpContext.Current.Request, fakeResponse);
                var fakeControllerContext = new ControllerContext(new HttpContextWrapper(fakeContext), this.ControllerContext.RouteData, this.ControllerContext.Controller);

                result.ExecuteResult(fakeControllerContext);

                memWriter.Flush();
            }
            return sb.ToString();
        }

        private string GetPathToWKHtmlToPdfExecutableOrThrow()
        {
            string path = Path.GetFullPath(Path.Combine(
                this.Server.MapPath("~"),
                AppSettings.Instance.WKHtmlToPdfExecutablePath));

            if (!System.IO.File.Exists(path))
                throw new ConfigurationErrorsException(string.Format("Path to wkhtmltopdf.exe is incorrect ({0}). Please install wkhtmltopdf.exe and/or update server configuration.", path));

            return path;
        }

        private PdfQuestionnaireModel LoadQuestionnaireOrThrow404(Guid id, Guid requestedByUserId, string requestedByUserName, Guid? translation, bool useDefaultTranslation)
        {
            PdfQuestionnaireModel questionnaire = this.pdfFactory.Load(id.FormatGuid(), requestedByUserId, requestedByUserName, translation, useDefaultTranslation);
            if (questionnaire == null)
            {
                throw new HttpResponseException(HttpStatusCode.NotFound);
            }
            return questionnaire;
        }

        private long GetFileSizeInKb(string filepath)
            => this.fileSystemAccessor.IsFileExists(filepath)
                ? this.fileSystemAccessor.GetFileSize(filepath) / 1024
                : 0;
    }
}
