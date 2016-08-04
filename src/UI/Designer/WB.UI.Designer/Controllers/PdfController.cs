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
using WB.UI.Designer.Pdf;
using WB.UI.Shared.Web.Filters;
using WB.UI.Shared.Web.Membership;

using AuthorizeAttribute = System.Web.Mvc.AuthorizeAttribute;

namespace WB.UI.Designer.Controllers
{
    public class PdfController : BaseController
    {
        private class PdfGenerationProgress : IDisposable
        {
            public MemoryStream Stream { get; } = new MemoryStream();
            public bool IsFailed { get; private set; }
            public bool IsFinished { get; private set; }

            public long SizeInKb => this.Stream.Length / 1024;

            public void Fail() => this.IsFailed = true;
            public void Finish() => this.IsFinished = true;

            public void Dispose() => this.Stream.Dispose();
        }

        private static readonly Dictionary<Guid, PdfGenerationProgress> GeneratedPdfs = new Dictionary<Guid, PdfGenerationProgress>();

        private readonly IPdfFactory pdfFactory;
        private readonly PdfSettings pdfSettings;
        private readonly ILogger logger;

        public PdfController(
            IMembershipUserService userHelper, 
            IPdfFactory pdfFactory, 
            PdfSettings pdfSettings,
            ILogger logger)
            : base(userHelper)
        {
            this.pdfFactory = pdfFactory;
            this.pdfSettings = pdfSettings;
            this.logger = logger;
        }

        [LocalOrDevelopmentAccessOnly]
        public ActionResult RenderQuestionnaire(Guid id, Guid requestedByUserId, string requestedByUserName)
        {
            var questionnaire = this.LoadQuestionnaire(id, requestedByUserId, requestedByUserName);
            return this.View("RenderQuestionnaire", questionnaire);
        }

        [LocalOrDevelopmentAccessOnly]
        public ActionResult RenderQuestionnaireFooter(Guid id)
        {
            return this.View("RenderQuestionnaireFooter");
        }

        [Authorize]
        public ActionResult PrintPreview(Guid id)
        {
            PdfQuestionnaireModel questionnaire = this.LoadQuestionnaire(id, UserHelper.WebUser.UserId, UserHelper.WebUser.UserName);
            return this.View("RenderQuestionnaire", questionnaire);
        }

        [Authorize]
        public ActionResult ExportQuestionnaire(Guid id)
        {
            var questionnaireTitle = this.pdfFactory.LoadQuestionnaireTitle(id);

            using (var memoryStream = new MemoryStream())
            {
                this.RenderQuestionnairePdfToMemoryStream(id, memoryStream);

                return this.File(memoryStream.ToArray(), "application/pdf", $"{questionnaireTitle}.pdf");
            }
        }

        [Authorize]
        [OutputCache(VaryByParam = "*", Duration = 0, NoStore = true)]
        public string Status(Guid id)
        {
            PdfGenerationProgress existingPdfGenerationProgress;

            if (GeneratedPdfs.TryGetValue(id, out existingPdfGenerationProgress))
            {
                if (existingPdfGenerationProgress.IsFailed)
                    return "Failed to generate PDF. Please reload the page and try again or contact support@mysurvey.solutions";

                return existingPdfGenerationProgress.IsFinished
                    ? $"Your PDF is ready. Size: {existingPdfGenerationProgress.SizeInKb}Kb"
                    : $"Your PDF is being generated. Size: {existingPdfGenerationProgress.SizeInKb}Kb";
            }
            else
            {
                var newPdfGenerationProgress = new PdfGenerationProgress();
                this.StartRenderPdf(id, newPdfGenerationProgress);
                GeneratedPdfs[id] = newPdfGenerationProgress;
                return "Started PDF generation";
            }
        }

        private void StartRenderPdf(Guid id, PdfGenerationProgress generationProgress)
        {
            var pdfConvertEnvironment = this.GetPdfConvertEnvironment();
            var pdfDocument = this.GetPdfConvertUrls(id);
            var pdfOutput = GetPdfConvertOutput(generationProgress.Stream);

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

        private void RenderQuestionnairePdfToMemoryStream(Guid id, MemoryStream memoryStream)
        {
            var pdfConvertEnvironment = this.GetPdfConvertEnvironment();
            var pdfDocument = this.GetPdfConvertUrls(id);
            var pdfOutput = GetPdfConvertOutput(memoryStream);

            PdfConvert.ConvertHtmlToPdf(pdfDocument, pdfConvertEnvironment, pdfOutput);
        }

        private PdfConvertEnvironment GetPdfConvertEnvironment() => new PdfConvertEnvironment
        {
            Timeout = this.pdfSettings.PdfGenerationTimeoutInMilliseconds,
            TempFolderPath = Path.GetTempPath(),
            WkHtmlToPdfPath = this.GetPathToWKHtmlToPdfExecutableOrThrow()
        };

        private PdfDocument GetPdfConvertUrls(Guid id) => new PdfDocument
        {
            Url = GlobalHelper.GenerateUrl("RenderQuestionnaire", "Pdf", new
            {
                id = id,
                requestedByUserId = this.UserHelper.WebUser.UserId,
                requestedByUserName = this.UserHelper.WebUser.UserName,
            }),
            FooterUrl = GlobalHelper.GenerateUrl("RenderQuestionnaireFooter", "Pdf", new
            {
                id = id
            }),
        };

        private static PdfOutput GetPdfConvertOutput(MemoryStream memoryStream) => new PdfOutput
        {
            OutputStream = memoryStream,
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

        private PdfQuestionnaireModel LoadQuestionnaire(Guid id, Guid requestedByUserId, string requestedByUserName)
        {
            PdfQuestionnaireModel questionnaire = this.pdfFactory.Load(id, requestedByUserId, requestedByUserName);
            if (questionnaire == null)
            {
                throw new HttpResponseException(HttpStatusCode.NotFound);
            }
            return questionnaire;
        }
    }
}