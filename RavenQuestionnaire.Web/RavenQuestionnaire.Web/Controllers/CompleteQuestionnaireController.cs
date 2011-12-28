using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using Questionnaire.Core.Web.Helpers;
using Questionnaire.Core.Web.Security;
using RavenQuestionnaire.Core;
using RavenQuestionnaire.Core.Commands;
using RavenQuestionnaire.Core.Entities.SubEntities;
using RavenQuestionnaire.Core.Views.CompleteQuestionnaire;
using RavenQuestionnaire.Core.Views.Questionnaire;
using RavenQuestionnaire.Core.Views.Status;
using RavenQuestionnaire.Core.Views.User;

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
            input.ResponsibleId = Global.GetCurrentUser().Id;
            var model = viewRepository.Load<CompleteQuestionnaireBrowseInputModel, CompleteQuestionnaireBrowseView>(input);
            return View(model);
        }

        public ViewResult Result(string id)
        {
            if (string.IsNullOrEmpty(id))
                throw new HttpException(404, "Invalid query string parameters");
            var model = viewRepository.Load<CompleteQuestionnaireViewInputModel, 
                CompleteQuestionnaireView>(new CompleteQuestionnaireViewInputModel(id));

            if (model != null)
                AddAllowedStatusesToViewBag(model.Status.Id, model.Status.Name);

            AddUsersToViewBag();
            return View(model);
        }

        [QuestionnaireAuthorize(UserRoles.Administrator, UserRoles.Supervisor, UserRoles.Operator)]
        public ActionResult Participate(string id)
        {
            var statusView = viewRepository.Load<StatusViewInputModel, StatusView>(new StatusViewInputModel(true));

            var command = new CreateNewCompleteQuestionnaireCommand(id, 
                                                                    Global.GetCurrentUser(),
                                                                    new SurveyStatus(statusView.Id, statusView.Title),
                                                                    Global.GetCurrentUser());
            commandInvoker.Execute(command);


            return RedirectToAction("Question",
                                    new
                                    {
                                        id = command.CompleteQuestionnaireId
                                    });
        }

        [QuestionnaireAuthorize(UserRoles.Administrator, UserRoles.Supervisor, UserRoles.Operator)]
        public ViewResult Question(string id, Guid? group)
        {
            if (string.IsNullOrEmpty(id))
                throw new HttpException(404, "Invalid query string parameters");
            var model =
                viewRepository.Load<CompleteQuestionnaireViewInputModel, CompleteQuestionnaireViewEnumerable>(
                    new CompleteQuestionnaireViewInputModel(id) { CurrentGroupPublicKey = group });
            return View( model);
        }

        public ActionResult SaveSingleResult(string id, Guid? PublicKey, CompleteAnswer[] answers)
        {
            if (answers == null || answers.Length <= 0)
            {
                return RedirectToAction("Question", new {id = id});
            }

            if (ModelState.IsValid)
            {
                commandInvoker.Execute(new UpdateAnswerInCompleteQuestionnaireCommand(id, PublicKey, answers,
                                                                                      Global.GetCurrentUser()));
            }
            return RedirectToAction("Question", new {id = id, group = PublicKey});
        }

        public ActionResult Delete(string id)
        {
            commandInvoker.Execute(new DeleteCompleteQuestionnaireCommand(id, Global.GetCurrentUser()));
            return RedirectToAction("Index");
        }


        protected void AddUsersToViewBag()
        {
            var users =
                viewRepository.Load<UserBrowseInputModel, UserBrowseView>(new UserBrowseInputModel() { PageSize = 300 }).Items;
            List<UserBrowseItem> list = users.ToList();
            ViewBag.Users = list;
        }


        protected void AddAllowedStatusesToViewBag(string statusId, string statusName)
        {
            List<SurveyStatus> statuses = new List<SurveyStatus>();

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

            SurveyStatus currentStatus = new SurveyStatus(statusId, statusName );
            if (!statuses.Contains(currentStatus))
                statuses.Add(currentStatus);

            ViewBag.AvailableStatuses = statuses;
        }
    }
}
