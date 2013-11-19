﻿using System;
using System.Configuration;
using System.IO;
using System.Web.Mvc;
using Main.Core.View;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.Pdf;
using WB.UI.Designer.Pdf;
using WB.UI.Shared.Web.Membership;

namespace WB.UI.Designer.Controllers
{
    public class PdfController : BaseController
    {
        private readonly IViewFactory<PdfQuestionnaireInputModel, PdfQuestionnaireView> pdfViewFactory;

        public PdfController(
            IMembershipUserService userHelper,
            IViewFactory<PdfQuestionnaireInputModel, PdfQuestionnaireView> viewFactory)
            : base(userHelper)
        {
            this.pdfViewFactory = viewFactory;
        }

        public ActionResult RenderQuestionnaire(Guid id)
        {
            PdfQuestionnaireView questionnaire = this.LoadQuestionnaire(id);

            return this.View(questionnaire);
        }

        public ActionResult RenderTitlePage(Guid id)
        {
            PdfQuestionnaireView questionnaire = this.LoadQuestionnaire(id);

            return this.View(questionnaire);
        }

        [Authorize]
        public ActionResult ExportQuestionnaire(Guid id)
        {
            PdfQuestionnaireView questionnaire = this.LoadQuestionnaire(id);

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
                        Url = GlobalHelper.GenerateUrl("RenderQuestionnaire", "Pdf", new { id = id }),
                        CoverUrl = GlobalHelper.GenerateUrl("RenderTitlePage", "Pdf", new {id = id})
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


        private PdfQuestionnaireView LoadQuestionnaire(Guid id)
        {
            var result = this.pdfViewFactory.Load(new PdfQuestionnaireInputModel() {Id = id});
            result.ReconnectWithParent();
            return result;
        }
    }
}