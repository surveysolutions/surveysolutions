namespace RavenQuestionnaire.Web.Controllers
{
    using System;
    using System.Configuration;
    using System.IO;
    using System.Web.Mvc;

    using Codaxy.WkHtmlToPdf;

    using Main.Core.View;

    using RavenQuestionnaire.Core.Views.Questionnaire;

    public class PdfController : Controller
    {
        private readonly IViewRepository viewRepository;

        public PdfController(IViewRepository viewRepository)
        {
            this.viewRepository = viewRepository;
        }

        [Authorize]
        public ActionResult PreviewQuestionnaire(Guid id)
        {
            QuestionnaireView questionnaire = this.LoadQuestionnaire(id);

            return this.View(questionnaire);
        }

        public ActionResult RenderQuestionnaire(Guid id)
        {
            QuestionnaireView questionnaire = this.LoadQuestionnaire(id);

            return this.View(questionnaire);
        }

        [Authorize]
        public ActionResult ExportQuestionnaire(Guid id)
        {
            QuestionnaireView questionnaire = this.LoadQuestionnaire(id);

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
                    Url = string.Format("{0}/Pdf/RenderQuestionnaire/{1}",
                        ConfigurationManager.AppSettings["SiteRoot"],
                        id),
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
                ConfigurationManager.AppSettings["WKHtmlToPdfExecutablePath"]));

            if (!System.IO.File.Exists(path))
                throw new ConfigurationErrorsException(string.Format("Path to wkhtmltopdf.exe is incorrect ({0}). Please install wkhtmltopdf.exe and/or update server configuration.", path));

            return path;
        }

        private QuestionnaireView LoadQuestionnaire(Guid id)
        {
            return this.viewRepository.Load<QuestionnaireViewInputModel, QuestionnaireView>(new QuestionnaireViewInputModel(id));
        }
    }
}