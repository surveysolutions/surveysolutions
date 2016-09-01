using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Mvc;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.Pdf;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure.FileSystem;
using WB.UI.Designer.Code;
using WB.UI.Designer.Pdf;
using WB.UI.Shared.Web.Filters;
using WB.UI.Shared.Web.Membership;

using AuthorizeAttribute = System.Web.Mvc.AuthorizeAttribute;

namespace WB.UI.Designer.Controllers
{
    public class PdfController : BaseController
    {
        #region Subclasses

        private class PdfGenerationProgress
        {
            public string FilePath { get; } = Path.GetTempFileName();
            public bool IsFailed { get; private set; }
            public bool IsFinished { get; private set; }

            public void Fail() => this.IsFailed = true;
            public void Finish() => this.IsFinished = true;
        }

        public class PdfStatus
        {
            private PdfStatus(string message, bool readyForDownload = false)
            {
                this.Message = message;
                this.ReadyForDownload = readyForDownload;
            }

            public string Message { get; }
            public bool ReadyForDownload { get; }

            public static PdfStatus InProgress(string message) => new PdfStatus(message);
            public static PdfStatus Failed(string message) => new PdfStatus(message);
            public static PdfStatus Ready(string message) => new PdfStatus(message, readyForDownload: true);
        }

        #endregion

        private static readonly Dictionary<Guid, PdfGenerationProgress> GeneratedPdfs = new Dictionary<Guid, PdfGenerationProgress>();

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
        public ActionResult RenderQuestionnaire(Guid id, Guid requestedByUserId, string requestedByUserName)
        {
            var questionnaire = this.LoadQuestionnaireOrThrow404(id, requestedByUserId, requestedByUserName);
            return this.View("RenderQuestionnaire", questionnaire);
        }

        [LocalOrDevelopmentAccessOnly]
        public ActionResult RenderQuestionnaireFooter(Guid id)
        {
            return this.View("RenderQuestionnaireFooter");
        }

        [CustomAuthorize]
        public ActionResult PrintPreview(Guid id)
        {
            PdfQuestionnaireModel questionnaire = this.LoadQuestionnaireOrThrow404(id, UserHelper.WebUser.UserId, UserHelper.WebUser.UserName);
            return this.View("RenderQuestionnaire", questionnaire);
        }

        [CustomAuthorize]
        public ActionResult Download(Guid id)
        {
            PdfGenerationProgress pdfGenerationProgress = GeneratedPdfs.GetOrNull(id);

            if (pdfGenerationProgress?.IsFinished == true)
            {
                var questionnaireTitle = this.pdfFactory.LoadQuestionnaireTitle(id);

                byte[] content = this.fileSystemAccessor.ReadAllBytes(pdfGenerationProgress.FilePath);

                this.fileSystemAccessor.DeleteFile(pdfGenerationProgress.FilePath);
                GeneratedPdfs.Remove(id);

                return this.File(content, "application/pdf", $"{questionnaireTitle}.pdf");
            }
            else
            {
                return this.HttpNotFound();
            }
        }

        [CustomAuthorize]
        [OutputCache(VaryByParam = "*", Duration = 0, NoStore = true)]
        [System.Web.Mvc.HttpGet]
        public JsonResult Status(Guid id)
        {
            PdfGenerationProgress existingPdfGenerationProgress;

            if (GeneratedPdfs.TryGetValue(id, out existingPdfGenerationProgress))
            {
                if (existingPdfGenerationProgress.IsFailed)
                    return this.Json(PdfStatus.Failed("Failed to generate PDF.\r\nPlease reload the page and try again or contact support@mysurvey.solutions"), JsonRequestBehavior.AllowGet);

                long sizeInKb = this.GetFileSizeInKb(existingPdfGenerationProgress.FilePath);

                if (sizeInKb == 0)
                    return this.Json(PdfStatus.InProgress("Preparing to generate your PDF.\r\nPlease wait..."), JsonRequestBehavior.AllowGet);

                return existingPdfGenerationProgress.IsFinished
                    ? this.Json(PdfStatus.Ready($"PDF document generated.\r\nSize: {sizeInKb}Kb"), JsonRequestBehavior.AllowGet)
                    : this.Json(PdfStatus.InProgress($"Your PDF is being generated.\r\nSize: {sizeInKb}Kb"), JsonRequestBehavior.AllowGet);
            }
            else
            {
                var newPdfGenerationProgress = new PdfGenerationProgress();
                this.StartRenderPdf(id, newPdfGenerationProgress);
                GeneratedPdfs[id] = newPdfGenerationProgress;
                return this.Json(PdfStatus.InProgress("PDF generation requested.\r\nPlease wait..."), JsonRequestBehavior.AllowGet);
            }
        }

        private void StartRenderPdf(Guid id, PdfGenerationProgress generationProgress)
        {
            var pdfConvertEnvironment = this.GetPdfConvertEnvironment();
            var pdfDocument = this.GetSourceUrlsForPdf(id);
            var pdfOutput = new PdfOutput { OutputFilePath = generationProgress.FilePath };

            Task.Factory.StartNew(() =>
            {
                try
                {
                    PdfConvert.ConvertHtmlToPdf(pdfDocument, pdfConvertEnvironment, pdfOutput);
                    generationProgress.Finish();
                }
                catch (Exception exception)
                {
                    this.logger.Error($"Failed to generate PDF {id.FormatGuid()}", exception);
                    generationProgress.Fail();
                }
            });
        }

        private PdfConvertEnvironment GetPdfConvertEnvironment() => new PdfConvertEnvironment
        {
            Timeout = this.pdfSettings.PdfGenerationTimeoutInMilliseconds,
            TempFolderPath = Path.GetTempPath(),
            WkHtmlToPdfPath = this.GetPathToWKHtmlToPdfExecutableOrThrow()
        };

        private PdfDocument GetSourceUrlsForPdf(Guid id) => new PdfDocument
        {
            Url = GlobalHelper.GenerateUrl(nameof(RenderQuestionnaire), "Pdf", new
            {
                id = id,
                requestedByUserId = this.UserHelper.WebUser.UserId,
                requestedByUserName = this.UserHelper.WebUser.UserName,
            }),
            FooterUrl = GlobalHelper.GenerateUrl(nameof(RenderQuestionnaireFooter), "Pdf", new
            {
                id = id
            }),
        };

        private string GetPathToWKHtmlToPdfExecutableOrThrow()
        {
            string path = Path.GetFullPath(Path.Combine(
                this.Server.MapPath("~"),
                AppSettings.Instance.WKHtmlToPdfExecutablePath));

            if (!System.IO.File.Exists(path))
                throw new ConfigurationErrorsException(string.Format("Path to wkhtmltopdf.exe is incorrect ({0}). Please install wkhtmltopdf.exe and/or update server configuration.", path));

            return path;
        }

        private PdfQuestionnaireModel LoadQuestionnaireOrThrow404(Guid id, Guid requestedByUserId, string requestedByUserName)
        {
            PdfQuestionnaireModel questionnaire = this.pdfFactory.Load(id, requestedByUserId, requestedByUserName);
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