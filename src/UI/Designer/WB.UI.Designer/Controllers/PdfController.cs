
namespace WB.UI.Designer.Controllers
{
    using Codaxy.WkHtmlToPdf;
    using Main.Core.View;
    using System;
    using System.Configuration;
    using System.IO;
    using System.Web.Mvc;
    
    using WB.UI.Designer.Views.Questionnaire;

    public class PdfController : BaseController
    {
         public PdfController(
             IViewRepository repository,
             IUserHelper userHelper)
             : base(repository, null, userHelper)
         {
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
                        Url = GlobalHelper.GenerateUrl("RenderQuestionnaire", "Pdf", new {id = id}),
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

        private QuestionnaireView LoadQuestionnaire(Guid id)
        {
            return this.Repository.Load<QuestionnaireViewInputModel, QuestionnaireView>(new QuestionnaireViewInputModel(id));
        }
    }
}