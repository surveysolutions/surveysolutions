using System.Web;
using System.Web.Mvc;
using Questionnaire.Core.Web.Helpers;
using Questionnaire.Core.Web.Security;
using RavenQuestionnaire.Core;
using RavenQuestionnaire.Core.Commands;
using RavenQuestionnaire.Core.Entities.SubEntities;
using RavenQuestionnaire.Core.Views.Questionnaire;

namespace RavenQuestionnaire.Web.Controllers
{
    [Authorize]
    public class QuestionnaireController : Controller
    {
        private ICommandInvoker commandInvoker;
        private IViewRepository viewRepository;
        public QuestionnaireController(ICommandInvoker commandInvoker, IViewRepository viewRepository)
        {
            this.commandInvoker = commandInvoker;
            this.viewRepository = viewRepository;
        }

        public ViewResult Index(QuestionnaireBrowseInputModel input)
        {
            var model = viewRepository.Load<QuestionnaireBrowseInputModel, QuestionnaireBrowseView>(input);
            return View(model);
        }
        /*public ActionResult Index()
        {
            var model = viewRepository.Load<QuestionnaireBrowseInputModel, QuestionnaireBrowseView>(new QuestionnaireBrowseInputModel());
            return View(model);
        }*/
        //
        // GET: /Questionnaire/Details/5

        public ViewResult Details(string id)
        {
            if (string.IsNullOrEmpty(id))
                throw new HttpException(404, "Invalid quesry string parameters");
            var model = viewRepository.Load<QuestionnaireViewInputModel, QuestionnaireView>(new QuestionnaireViewInputModel(id));
            return View(model);
        }



        //
        // GET: /Questionnaire/Create
        [QuestionnaireAuthorize(UserRoles.Administrator, UserRoles.Supervisor)]
        public ActionResult Create()
        {
            return View(new QuestionnaireView());
        }

        [QuestionnaireAuthorize(UserRoles.Administrator)]
        public ActionResult Edit(string id)
        {
            if (string.IsNullOrEmpty(id))
                throw new HttpException(404, "Invalid query string parameters.");
            var model = viewRepository.Load<QuestionnaireViewInputModel, QuestionnaireView>(new QuestionnaireViewInputModel(id));
            return View("Create", model);
        }

        //
        // POST: /Questionnaire/Create

        [HttpPost]
        [QuestionnaireAuthorize(UserRoles.Administrator)]
        public ActionResult Save(QuestionnaireView model)
        {
            if (ModelState.IsValid)
            {
                if (string.IsNullOrEmpty(model.Id))
                {
                    commandInvoker.Execute(new CreateNewQuestionnaireCommand(model.Title, Global.GetCurrentUser()));
                }
                else
                {
                    commandInvoker.Execute(new UpdateQuestionnaireCommand(model.Id, model.Title, Global.GetCurrentUser()));
                }
                return RedirectToAction("Index");

            }
            return View("Create", model);
        }


        //
        // GET: /Questionnaire/Delete/5
        [QuestionnaireAuthorize(UserRoles.Administrator)]
        public ActionResult Delete(string id)
        {
            commandInvoker.Execute(new DeleteQuestionnaireCommand(id, Global.GetCurrentUser()));
            return RedirectToAction("Index");
        }

    }
}
