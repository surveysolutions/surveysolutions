namespace RavenQuestionnaire.Web.Controllers
{
    using System;
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
                RenderQuestionnairePdfToMemoryStream(id, memoryStream);

                return this.File(memoryStream.ToArray(), "application/pdf", string.Format("{0}.pdf", questionnaire.Title));
            }
        }

        private static void RenderQuestionnairePdfToMemoryStream(Guid id, MemoryStream memoryStream)
        {
            PdfConvert.Environment.WkHtmlToPdfPath = @"C:\Program Files (x86)\wkhtmltopdf\wkhtmltopdf.exe";

            PdfConvert.ConvertHtmlToPdf(
                new PdfDocument
                {
                    Url = string.Format("http://localhost/RavenQuestionnaire.Web/Pdf/RenderQuestionnaire/{0}", id),
                },
                new PdfOutput
                {
                    OutputStream = memoryStream,
                });

            memoryStream.Flush();
        }

        private QuestionnaireView LoadQuestionnaire(Guid id)
        {
            return this.viewRepository.Load<QuestionnaireViewInputModel, QuestionnaireView>(new QuestionnaireViewInputModel(id));
        }
    }
}