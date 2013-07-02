using Main.Core.View;
using System;
using System.Configuration;
using System.Globalization;
using System.IO;
using System.Web.Mvc;
using WB.UI.Designer.Pdf;
using WB.UI.Designer.Providers.CQRS.Accounts.View;
using WB.UI.Designer.Views.Questionnaire;
using WB.UI.Shared.Web.Membership;

namespace WB.UI.Designer.Controllers
{
    public class PdfController : BaseController
    {
        private readonly IViewFactory<AccountViewInputModel, AccountView> userViewFactory;

        public PdfController(IMembershipUserService userHelper,
            IViewFactory<AccountViewInputModel, AccountView> userViewFactory)
            : base(userHelper)
        {
            this.userViewFactory = userViewFactory;
        }

        public ActionResult RenderQuestionnaire(Guid id)
        {
            QuestionnaireView questionnaire = this.LoadQuestionnaire(id);
            ViewBag.QuestionnaireGuid = questionnaire.PublicKey;

            return this.View(questionnaire);
        }

        public ActionResult RenderTitlePage(Guid id)
        {
            QuestionnaireView questionnaire = this.LoadQuestionnaire(id);

            var model = new TitlePageDto();
            model.SurveyName = questionnaire.Title;
            model.CreationDate = questionnaire.CreationDate.ToString(CultureInfo.InvariantCulture.DateTimeFormat.ShortDatePattern);
            model.ChaptersCount = questionnaire.GetChaptersCount();
            model.QuestionsCount = questionnaire.GetQuestionsCount();
            model.QuestionsWithConditionsCount = questionnaire.GetQuestionsWithConditionsCount();
            model.GroupsCount = questionnaire.GetGroupsCount();

            if (questionnaire.CreatedBy.HasValue)
            {
                AccountView accountView = userViewFactory.Load(new AccountViewInputModel(questionnaire.CreatedBy.Value));
                if (accountView != null)
                {
                    model.AuthorName = accountView.UserName;
                }
                else
                {
                    model.AuthorName = "No Author";
                }
            }
            else
            {
                model.AuthorName = "No Author";
            }

            return this.View(model);
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

        private QuestionnaireView LoadQuestionnaire(Guid id)
        {
            return this.Repository.Load<QuestionnaireViewInputModel, QuestionnaireView>(new QuestionnaireViewInputModel(id));
        }
    }
}