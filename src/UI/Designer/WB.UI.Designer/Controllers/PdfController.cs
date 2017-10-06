﻿using System;
using System.Collections.Concurrent;
using System.Configuration;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Mvc;
using WB.Core.BoundedContexts.Designer.Implementation.Services.Accounts.Membership;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.Pdf;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure.FileSystem;
using WB.UI.Designer.Pdf;
using WB.UI.Designer.Resources;
using WB.UI.Shared.Web.Filters;

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

        private static readonly ConcurrentDictionary<Guid, PdfGenerationProgress> GeneratedPdfs = new ConcurrentDictionary<Guid, PdfGenerationProgress>();

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
        public ActionResult RenderQuestionnaire(Guid id, Guid requestedByUserId, string requestedByUserName)
        {
            var questionnaire = this.LoadQuestionnaireOrThrow404(id, requestedByUserId, requestedByUserName);
            return this.View("RenderQuestionnaire", questionnaire);
        }

        [LocalOrDevelopmentAccessOnly]
        [System.Web.Mvc.AllowAnonymous]
        public ActionResult RenderQuestionnaireFooter(Guid id)
        {
            return this.View("RenderQuestionnaireFooter");
        }

        public ActionResult PrintPreview(Guid id)
        {
            PdfQuestionnaireModel questionnaire = this.LoadQuestionnaireOrThrow404(id, UserHelper.WebUser.UserId, UserHelper.WebUser.UserName);
            return this.View("RenderQuestionnaire", questionnaire);
        }

        public ActionResult Download(Guid id)
        {
            PdfGenerationProgress pdfGenerationProgress = GeneratedPdfs.GetOrNull(id);

            if (pdfGenerationProgress?.IsFinished == true)
            {
                var questionnaireTitle = this.pdfFactory.LoadQuestionnaireTitle(id);

                byte[] content = this.fileSystemAccessor.ReadAllBytes(pdfGenerationProgress.FilePath);

                this.fileSystemAccessor.DeleteFile(pdfGenerationProgress.FilePath);
                GeneratedPdfs.TryRemove(id);

                return this.File(content, "application/pdf", $"{questionnaireTitle}.pdf");
            }
            else
            {
                return this.HttpNotFound();
            }
        }

        [OutputCache(VaryByParam = "*", Duration = 0, NoStore = true)]
        [System.Web.Mvc.HttpGet]
        public JsonResult Status(Guid id)
        {
            PdfGenerationProgress pdfGenerationProgress = GeneratedPdfs.GetOrAdd(id, StartNewPdfGeneration);

            if (pdfGenerationProgress.IsFailed)
                return this.Json(PdfStatus.Failed(PdfMessages.FailedToGenerate), JsonRequestBehavior.AllowGet);

            long sizeInKb = this.GetFileSizeInKb(pdfGenerationProgress.FilePath);

            if (sizeInKb == 0)
                return this.Json(PdfStatus.InProgress(PdfMessages.PreparingToGenerate), JsonRequestBehavior.AllowGet);

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
        public ActionResult Retry(Guid id)
        {
            PdfGenerationProgress pdfGenerationProgress = GeneratedPdfs.GetOrAdd(id, StartNewPdfGeneration);
            if (pdfGenerationProgress != null && pdfGenerationProgress.IsFailed)
            {
                GeneratedPdfs.TryRemove(id);
            }
            GeneratedPdfs.GetOrAdd(id, this.StartNewPdfGeneration);
            return this.Json(PdfStatus.InProgress(PdfMessages.Retry));
        }

        private PdfGenerationProgress StartNewPdfGeneration(Guid id)
        {
            var newPdfGenerationProgress = new PdfGenerationProgress();
            this.StartRenderPdf(id, newPdfGenerationProgress);
            return newPdfGenerationProgress;
        }


        private void StartRenderPdf(Guid id, PdfGenerationProgress generationProgress)
        {
            var pdfConvertEnvironment = this.GetPdfConvertEnvironment();
            var pdfDocument = this.GetSourceUrlsForPdf(id);
            var pdfOutput = new PdfOutput {OutputFilePath = generationProgress.FilePath};
            Task.Run(() =>
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
            PdfQuestionnaireModel questionnaire = this.pdfFactory.Load(id.FormatGuid(), requestedByUserId, requestedByUserName);
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