using System.Collections.Generic;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using Questionnaire.Core.Web.Membership;
using RavenQuestionnaire.Core;
using RavenQuestionnaire.Core.Commands;
using RavenQuestionnaire.Core.Entities.SubEntities;
using RavenQuestionnaire.Core.Views.CompleteQuestionnaire;
using RavenQuestionnaire.Core.Views.Questionnaire;
using RavenQuestionnaire.Core.Views.Status;

namespace RavenQuestionnaire.Web.Controllers
{
    [Authorize]
    public class CompleteQuestionnaireController : Controller
    {
        private ICommandInvoker commandInvoker;
        private IViewRepository viewRepository;
        private IFormsAuthentication authentication;

        public CompleteQuestionnaireController(ICommandInvoker commandInvoker, IViewRepository viewRepository, IFormsAuthentication auth)
        {
            this.commandInvoker = commandInvoker;
            this.viewRepository = viewRepository;
            this.authentication = auth;
        }

        public ViewResult Index(CompleteQuestionnaireBrowseInputModel input)
        {
            var model = viewRepository.Load<CompleteQuestionnaireBrowseInputModel, CompleteQuestionnaireBrowseView>(input);
            return View(model);
        }


        public ViewResult MyItems(CompleteQuestionnaireBrowseInputModel input)
        {
            input.ResponsibleId = authentication.GetUserIdForCurrentUser();
            var model = viewRepository.Load<CompleteQuestionnaireBrowseInputModel, CompleteQuestionnaireBrowseView>(input);
            return View(model);
        }

        public ViewResult Result(string id)
        {
            if (string.IsNullOrEmpty(id))
                throw new HttpException(404, "Invalid query string parameters");
            var model = viewRepository.Load<CompleteQuestionnaireViewInputModel, CompleteQuestionnaireView>(new CompleteQuestionnaireViewInputModel(id));
            if (model != null)
                AddAllowedStatusesToViewBag(model.Status);
            return View(model);
        }

        public ActionResult SaveResult(string id, CompleteAnswer[] answers)
        {
            if (ModelState.IsValid)
            {
                commandInvoker.Execute(new CreateNewCompleteQuestionnaireCommand(id, answers, authentication.GetUserIdForCurrentUser()));

            }
            return RedirectToAction("Index");
        }

        public ActionResult UpdateResult(string id, CompleteAnswer[] answers)
        {
            if (ModelState.IsValid)
            {
                commandInvoker.Execute(new UpdateCompleteQuestionnaireCommand(id, answers));

            }
            return RedirectToAction("Index");
        }

        [QuestionnaireAuthorize(UserRoles.Administrator, UserRoles.Supervisor, UserRoles.Operator)]
        public ActionResult Take(string id)
        {
            if (string.IsNullOrEmpty(id))
                throw new HttpException(404, "Invalid query string parameters");

            var model = viewRepository.Load<QuestionnaireViewInputModel, QuestionnaireView>(
                new QuestionnaireViewInputModel(id));
            return View( new CompleteQuestionnaireView(model));
        }

        public ActionResult Delete(string id)
        {
            commandInvoker.Execute(new DeleteCompleteQuestionnaireCommand(id));
            return RedirectToAction("Index");
        }



        protected void AddAllowedStatusesToViewBag(string statusId)
        {
            List<string> statuses = new List<string>();

            StatusView model = viewRepository.Load<StatusViewInputModel, StatusView>(new StatusViewInputModel(statusId));
            if (model != null)
            {
                foreach (var role in Roles.GetRolesForUser())
                {
                    if (model.StatusRoles.ContainsKey(role))
                        foreach (var item in model.StatusRoles[role])
                        {
                            if (!statuses.Contains(item))
                                statuses.Add(item);
                        }
                }
            }

            if (!statuses.Contains(statusId))
                statuses.Add(statusId);

            ViewBag.AvailableStatuses = statuses;
        }
    }
}
