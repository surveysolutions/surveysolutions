namespace RavenQuestionnaire.Web.Controllers
{
    using System;
    using System.Web.Mvc;

    using Main.Core.View;

    using RavenQuestionnaire.Core.Views.Questionnaire;

    [Authorize]
    public class PdfController : Controller
    {
        private readonly IViewRepository viewRepository;

        public PdfController(IViewRepository viewRepository)
        {
            this.viewRepository = viewRepository;
        }

        public ActionResult PreviewQuestionnaire(Guid id)
        {
            QuestionnaireView viewModel = this.LoadQuestionnaire(id);

            return this.View(viewModel);
        }

        private QuestionnaireView LoadQuestionnaire(Guid id)
        {
            return this.viewRepository.Load<QuestionnaireViewInputModel, QuestionnaireView>(new QuestionnaireViewInputModel(id));
        }
    }
}