using System;
using System.Collections.Generic;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using Questionnaire.Core.Web.Membership;
using Questionnaire.Core.Web.Security;
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
            var model = viewRepository.Load<CompleteQuestionnaireViewInputModel, 
                CompleteQuestionnaireView>(new CompleteQuestionnaireViewInputModel(id));

            if (model != null)
                AddAllowedStatusesToViewBag(model.Status.Id, model.Status.Name);
            return View(model);
        }

        public ActionResult SaveResult(string id, CompleteAnswer[] answers)
        {
            if (ModelState.IsValid)
            {
                var statusView = viewRepository.Load<StatusViewInputModel, StatusView>(new StatusViewInputModel(true));

                commandInvoker.Execute(new CreateNewCompleteQuestionnaireCommand(id,
                    answers, authentication.GetCurrentUser(), new SurveyStatus(statusView.Id, statusView.Title)));

            }
            return RedirectToAction("Index");
        }
        public ActionResult SaveFirstStep(string id, CompleteAnswer[] answers)
        {
            if (ModelState.IsValid)
            {
                var statusView = viewRepository.Load<StatusViewInputModel, StatusView>(new StatusViewInputModel(true));

                var command = new CreateNewCompleteQuestionnaireCommand(id, answers,
                                                                        authentication.GetCurrentUser(), 
                                                                        new SurveyStatus(statusView.Id, statusView.Title ));
                commandInvoker.Execute(command);


                return RedirectToAction("Question",
                                        new
                                            {
                                                id = command.CompleteQuestionnaireId,
                                                question = answers[0].QuestionPublicKey
                                            });
            }
            return RedirectToAction("Participate", new { id });
        }
        public ActionResult UpdateResult(string id, CompleteAnswer[] answers, SurveyStatus status)
        {
            if (ModelState.IsValid)
            {
                commandInvoker.Execute(new UpdateCompleteQuestionnaireCommand(id, answers, status.Id));

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
            return View(new CompleteQuestionnaireView(model));
        }
        [QuestionnaireAuthorize(UserRoles.Administrator, UserRoles.Supervisor, UserRoles.Operator)]
        public ActionResult Participate(string id)
        {
            if (string.IsNullOrEmpty(id))
                throw new HttpException(404, "Invalid query string parameters");

            var model = viewRepository.Load<QuestionnaireViewInputModel, QuestionnaireView>(
                new QuestionnaireViewInputModel(id));

            ViewBag.ShowPrevious = false;
            return View(new CompleteQuestionnaireViewEnumerable(model));
        }

        [QuestionnaireAuthorize(UserRoles.Administrator, UserRoles.Supervisor, UserRoles.Operator)]
        public ActionResult Question(string id, Guid? question, bool? order)
        {
            if (string.IsNullOrEmpty(id))
                throw new HttpException(404, "Invalid query string parameters");
            var model =
                viewRepository.Load<CompleteQuestionnaireViewInputModel, CompleteQuestionnaireViewEnumerable>(
                    new CompleteQuestionnaireViewInputModel(id, question, order?? false));
            return View( model);
        }

        public ActionResult SaveSingleResult(string id, CompleteAnswer[] answers, string order)
        {
            if (answers == null || answers.Length <= 0)
            {
                return RedirectToAction("Question", new { id = id, order = order == "Previous" });
            }
            if (ModelState.IsValid)
            {
                commandInvoker.Execute(new UpdateAnswerInCompleteQuestionnaireCommand(id, answers[0]));
            }

            return RedirectToAction("Question", new { id = id, question = answers[0].QuestionPublicKey, order = order == "Previous"});
        }

        public ActionResult Delete(string id)
        {
            commandInvoker.Execute(new DeleteCompleteQuestionnaireCommand(id));
            return RedirectToAction("Index");
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
