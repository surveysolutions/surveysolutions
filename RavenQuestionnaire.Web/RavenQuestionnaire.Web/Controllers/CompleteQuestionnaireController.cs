using System;
using System.Collections.Generic;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using Questionnaire.Core.Web.Helpers;
using Questionnaire.Core.Web.Security;
using RavenQuestionnaire.Core;
using RavenQuestionnaire.Core.Commands;
using RavenQuestionnaire.Core.Entities.SubEntities;
using RavenQuestionnaire.Core.Entities.SubEntities.Complete;
using RavenQuestionnaire.Core.Views.CompleteQuestionnaire;
using RavenQuestionnaire.Core.Views.Group;
using RavenQuestionnaire.Core.Views.Questionnaire;
using RavenQuestionnaire.Core.Views.Status;

namespace RavenQuestionnaire.Web.Controllers
{
    [Authorize]
    public class CompleteQuestionnaireController : Controller
    {
        private ICommandInvoker commandInvoker;
        private IViewRepository viewRepository;
        private IBagManager _bagManager;
        private IGlobalInfoProvider _globalProvider;

        public CompleteQuestionnaireController(ICommandInvoker commandInvoker, IViewRepository viewRepository,
            IBagManager bagManager, IGlobalInfoProvider globalProvider)
        {
            this.commandInvoker = commandInvoker;
            this.viewRepository = viewRepository;
            this._bagManager = bagManager;
            this._globalProvider = globalProvider;
        }

        public ViewResult Index(CompleteQuestionnaireBrowseInputModel input)
        {
            var model = viewRepository.Load<CompleteQuestionnaireBrowseInputModel, CompleteQuestionnaireBrowseView>(input);
            return View(model);
        }

        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult _TableData(CompleteQuestionnaireBrowseInputModel input)
        {
            input.ResponsibleId = _globalProvider.GetCurrentUser().Id;
            var model = viewRepository.Load<CompleteQuestionnaireBrowseInputModel, CompleteQuestionnaireBrowseView>(input);
            return PartialView("_Table", model);
        }

        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult _TableIndexData(CompleteQuestionnaireBrowseInputModel input)
        {
            var model = viewRepository.Load<CompleteQuestionnaireBrowseInputModel, CompleteQuestionnaireBrowseView>(input);
            return PartialView("_TableIndex", model);
        }

        public ViewResult MyItems(CompleteQuestionnaireBrowseInputModel input)
        {
            input.ResponsibleId = _globalProvider.GetCurrentUser().Id;
            var model = viewRepository.Load<CompleteQuestionnaireBrowseInputModel, CompleteQuestionnaireBrowseView>(input);
            return View(model);
        }

        public ActionResult UpdateResult(string id, CompleteAnswer[] answers, SurveyStatus status, UserLight responsible)
        {
            if (ModelState.IsValid)
            {
                commandInvoker.Execute(new UpdateCompleteQuestionnaireCommand(id, /*answers,*/ status.Id, responsible.Id,
                    _globalProvider.GetCurrentUser()));

            }
            return RedirectToAction("Index");
        }

        public ViewResult Result(string id)
        {
            if (string.IsNullOrEmpty(id))
                throw new HttpException(404, "Invalid query string parameters");
            var model = viewRepository.Load<CompleteQuestionnaireViewInputModel, 
                CompleteQuestionnaireView>(new CompleteQuestionnaireViewInputModel(id));

            if (model != null)
                AddAllowedStatusesToViewBag(model.Status.Id, model.Status.Name);

            _bagManager.AddUsersToBag(ViewBag, viewRepository);
            return View(model);
        }

        [QuestionnaireAuthorize(UserRoles.Administrator, UserRoles.Supervisor, UserRoles.Operator)]
        public ActionResult Participate(string id)
        {
            var statusView = viewRepository.Load<StatusViewInputModel, StatusView>(new StatusViewInputModel(true));

            var command = new CreateNewCompleteQuestionnaireCommand(id,
                                                                    _globalProvider.GetCurrentUser(),
                                                                    new SurveyStatus(statusView.Id, statusView.Title),
                                                                    _globalProvider.GetCurrentUser());
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
            ViewBag.CurrentGroup = model.CurrentGroup;
            return View( model);
        }

        public ActionResult SaveSingleResult(string id, Guid? PublicKey, Guid? PropogationPublicKey, CompleteAnswer[] answers)
        {
            if (answers == null || answers.Length <= 0)
            {
                return RedirectToAction("Question", new {id = id});
            }

            if (ModelState.IsValid)
            {
                if (PropogationPublicKey.HasValue)
                {
                    for (int i = 0; i < answers.Length; i++)
                    {
                        answers[i] = new PropagatableCompleteAnswer(answers[i], PropogationPublicKey.Value);
                    }
                }
                commandInvoker.Execute(new UpdateAnswerInCompleteQuestionnaireCommand(id, answers,
                                                                                      _globalProvider.GetCurrentUser()));
            }
            var model =
                viewRepository.Load<CompleteGroupViewInputModel, CompleteGroupView>(
                    new CompleteGroupViewInputModel(PropogationPublicKey, PublicKey, id));
            ViewBag.CurrentGroup = model;
            return PartialView("~/Views/Group/_Screen.cshtml", model);
        }

        public ActionResult Delete(string id)
        {
            commandInvoker.Execute(new DeleteCompleteQuestionnaireCommand(id, _globalProvider.GetCurrentUser()));
            return RedirectToAction("Index");
        }

        protected void AddAllowedStatusesToViewBag(string statusId, string statusName)
        {
            List<SurveyStatus> statuses = new List<SurveyStatus>();

            bool isCurrentPresent = false;
            StatusView model = viewRepository.Load<StatusViewInputModel, StatusView>(new StatusViewInputModel(statusId));
            if (model != null)
            {
                foreach (var role in Roles.GetRolesForUser())
                {
                    if (model.StatusRoles.ContainsKey(role))
                        foreach (var item in model.StatusRoles[role])
                        {
                            if (statuses.Contains(item)) continue;
                            statuses.Add(item);
                            if (isCurrentPresent) continue;

                            if (item.Id == statusId && item.Name == statusName)
                                isCurrentPresent = true;
                        }
                }
            }
            if (!isCurrentPresent)
                statuses.Add(new SurveyStatus(statusId, statusName));

            ViewBag.AvailableStatuses = statuses;
        }
    }
}
