using System;
using System.Web;
using System.Web.Mvc;
using Ncqrs;
using Ncqrs.Commanding.ServiceModel;
using Questionnaire.Core.Web.Helpers;
using Questionnaire.Core.Web.Security;
using RavenQuestionnaire.Core;
using RavenQuestionnaire.Core.Commands;
using RavenQuestionnaire.Core.Commands.Location;
using RavenQuestionnaire.Core.Commands.Questionnaire;
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
                    commandInvoker.Execute(new CreateNewLocationCommand(model.Location, GlobalInfo.GetCurrentUser()));

                    var commandService = NcqrsEnvironment.Get<ICommandService>();
                    commandService.Execute(new CreateQuestionnaireCommand(Guid.NewGuid(), model.Location));
                }
                return RedirectToAction("Index");

            }
            return View("Create", model);
        }



    }
}
