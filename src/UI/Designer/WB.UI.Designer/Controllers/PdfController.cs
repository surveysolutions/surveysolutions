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

        public PdfController(
            IMembershipUserService userHelper, 
            IPdfFactory pdfFactory)
            : base(userHelper)
        {
            this.pdfFactory = pdfFactory;
        }

        [LocalOrDevelopmentAccessOnly]
        public ActionResult RenderQuestionnaire(Guid id, Guid requestedByUserId, string requestedByUserName)
        {
            var questionnaire = this.LoadQuestionnaire(id, requestedByUserId, requestedByUserName);
            return this.View("RenderQuestionnaire", questionnaire);
        }

        [System.Web.Mvc.Authorize]
        public ActionResult PrintPreview(Guid id)
        {
            var questionnaire = this.LoadQuestionnaire(id, UserHelper.WebUser.UserId, UserHelper.WebUser.UserName);
            return this.View("RenderQuestionnaire", questionnaire);
        }

        [System.Web.Mvc.Authorize]
        public ActionResult ExportQuestionnaire(Guid id)
        {
            // find other place to load questionnaire title, this view is to heavy for this
            var questionnaire = this.LoadQuestionnaire(id, UserHelper.WebUser.UserId, UserHelper.WebUser.UserName);

            using (var memoryStream = new MemoryStream())
            {
                this.RenderQuestionnairePdfToMemoryStream(id, memoryStream);

                return this.File(memoryStream.ToArray(), "application/pdf", string.Format("{0}.pdf", questionnaire.Title));
            }
        }

        private void RenderQuestionnairePdfToMemoryStream(Guid id, MemoryStream memoryStream)
        {
            PdfConvert.Environment.WkHtmlToPdfPath = this.GetPathToWKHtmlToPdfExecutableOrThrow();

            PdfConvert.ConvertHtmlToPdf(
                new PdfDocument
                    {
                        Url = GlobalHelper.GenerateUrl("RenderQuestionnaire", "Pdf", new { id = id, requestedByUserId = UserHelper.WebUser.UserId, requestedByUserName= UserHelper.WebUser.UserName })
                    },
                new PdfOutput
                    {
                        OutputStream = memoryStream,
                    });

            memoryStream.Flush();
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