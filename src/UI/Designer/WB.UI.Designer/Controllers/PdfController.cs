using System;
using System.Configuration;
using System.IO;
using System.Net;
using System.Web.Http;
using System.Web.Mvc;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.Pdf;
using WB.UI.Designer.Pdf;
using WB.UI.Shared.Web.Filters;
using WB.UI.Shared.Web.Membership;

namespace WB.UI.Designer.Controllers
{
    public class PdfController : BaseController
    {
        private readonly IPdfFactory pdfFactory;
        private readonly PdfSettings pdfSettings;

        public PdfController(
            IMembershipUserService userHelper, 
            IPdfFactory pdfFactory, 
            PdfSettings pdfSettings)
            : base(userHelper)
        {
            this.pdfFactory = pdfFactory;
            this.pdfSettings = pdfSettings;
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

        [System.Web.Mvc.Authorize]
        public ActionResult PrintPreview(Guid id)
        {
            PdfQuestionnaireModel questionnaire = this.LoadQuestionnaire(id, UserHelper.WebUser.UserId, UserHelper.WebUser.UserName);
            return this.View("RenderQuestionnaire", questionnaire);
        }

        [System.Web.Mvc.Authorize]
        public ActionResult ExportQuestionnaire(Guid id)
        {
            var questionnaireTitle = this.pdfFactory.LoadQuestionnaireTitle(id);

            using (var memoryStream = new MemoryStream())
            {
                this.RenderQuestionnairePdfToMemoryStream(id, memoryStream);

                return this.File(memoryStream.ToArray(), "application/pdf", $"{questionnaireTitle}.pdf");
            }
        }

        private void RenderQuestionnairePdfToMemoryStream(Guid id, MemoryStream memoryStream)
        {
            var pdfConvertEnvironment = new PdfConvertEnvironment
            {
                Timeout = pdfSettings.PdfGenerationTimeoutInMilliseconds,
                TempFolderPath = Path.GetTempPath(),
                WkHtmlToPdfPath = this.GetPathToWKHtmlToPdfExecutableOrThrow()
            };

            var pdfDocument = new PdfDocument
            {
                Url = GlobalHelper.GenerateUrl("RenderQuestionnaire", "Pdf", new { id = id, requestedByUserId = this.UserHelper.WebUser.UserId, requestedByUserName= this.UserHelper.WebUser.UserName }),
                FooterUrl = GlobalHelper.GenerateUrl("RenderQuestionnaireFooter", "Pdf", new { id = id})
            };

            var pdfOutput = new PdfOutput
            {
                OutputStream = memoryStream,
            };

            PdfConvert.ConvertHtmlToPdf(pdfDocument, pdfConvertEnvironment, pdfOutput);
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