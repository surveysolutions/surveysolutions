using System.Web;
using System.Web.Mvc;
using Questionnaire.Core.Web.Membership;
using RavenQuestionnaire.Core;
using RavenQuestionnaire.Core.Commands;
using RavenQuestionnaire.Core.Entities.SubEntities;
using RavenQuestionnaire.Core.Views.Location;
using RavenQuestionnaire.Core.Views.Questionnaire;

namespace RavenQuestionnaire.Web.Controllers
{
    [QuestionnaireAuthorize(UserRoles.Administrator)]
    public class LocationController : Controller
    {
        private ICommandInvoker commandInvoker;
        private IViewRepository viewRepository;
        public LocationController(ICommandInvoker commandInvoker, IViewRepository viewRepository)
        {
            this.commandInvoker = commandInvoker;
            this.viewRepository = viewRepository;
        }

        public ViewResult Index(LocationBrowseInputModel input)
        {
            var model = viewRepository.Load<LocationBrowseInputModel, LocationBrowseView>(input);
            return View(model);
        }
        public ActionResult Create()
        {
            return View(LocationBrowseItem.New());
        }

        public ActionResult Edit(string id)
        {
            if (string.IsNullOrEmpty(id))
                throw new HttpException(404, "Invalid query string parameters");
            var model = viewRepository.Load<QuestionnaireViewInputModel, QuestionnaireView>(new QuestionnaireViewInputModel(id));
            return View("Create", model);
        }

        //
        // POST: /Questionnaire/Create

        [HttpPost]
        public ActionResult Save(LocationBrowseItem model)
        {
            if (ModelState.IsValid)
            {
                if (string.IsNullOrEmpty(model.Id))
                {
                    commandInvoker.Execute(new CreateNewLocationCommand(model.Location));
                }
                else
                {
                    //   commandInvoker.Execute(new UpdateQuestionnaireCommand(model.Id, model.Title));
                }
                return RedirectToAction("Index");

            }
            return View("Create", model);
        }



    }
}
