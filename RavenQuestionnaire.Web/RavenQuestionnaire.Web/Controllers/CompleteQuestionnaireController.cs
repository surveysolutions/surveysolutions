#region

using System;
using System.Collections.Generic;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using Questionnaire.Core.Web.Helpers;
using Questionnaire.Core.Web.Security;
using RavenQuestionnaire.Core;
using RavenQuestionnaire.Core.Commands.Collection;
using RavenQuestionnaire.Core.Commands.Questionnaire.Completed;
using RavenQuestionnaire.Core.Commands.Questionnaire.Group;
using RavenQuestionnaire.Core.Entities.SubEntities;
using RavenQuestionnaire.Core.Views.CompleteQuestionnaire;
using RavenQuestionnaire.Core.Views.CompleteQuestionnaire.Mobile;
using RavenQuestionnaire.Core.Views.CompleteQuestionnaire.Vertical;
using RavenQuestionnaire.Core.Views.Group;
using RavenQuestionnaire.Core.Views.Question;
using RavenQuestionnaire.Core.Views.Status;
using RavenQuestionnaire.Web.Models;

#endregion

namespace RavenQuestionnaire.Web.Controllers
{
    [Authorize]
    public class CompleteQuestionnaireController : Controller
    {
        private readonly IBagManager _bagManager;
        private readonly IGlobalInfoProvider _globalProvider;
        private readonly ICommandInvoker commandInvoker;
        private readonly IViewRepository viewRepository;

        public CompleteQuestionnaireController(ICommandInvoker commandInvoker, IViewRepository viewRepository,
                                               IBagManager bagManager, IGlobalInfoProvider globalProvider)
        {
            this.commandInvoker = commandInvoker;
            this.viewRepository = viewRepository;
            _bagManager = bagManager;
            _globalProvider = globalProvider;
        }

        public ViewResult Index(CompleteQuestionnaireBrowseInputModel input)
        {
            var model =
                viewRepository.Load<CompleteQuestionnaireBrowseInputModel, CompleteQuestionnaireBrowseView>(input);
            return View(model);
        }

        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult _TableData(CompleteQuestionnaireBrowseInputModel input)
        {
            input.ResponsibleId = _globalProvider.GetCurrentUser().Id;
            var model =
                viewRepository.Load<CompleteQuestionnaireBrowseInputModel, CompleteQuestionnaireBrowseView>(input);
            return PartialView("_Table", model);
        }

        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult _TableIndexData(CompleteQuestionnaireBrowseInputModel input)
        {
            var model =
                viewRepository.Load<CompleteQuestionnaireBrowseInputModel, CompleteQuestionnaireBrowseView>(input);
            return PartialView("_TableIndex", model);
        }

        public ViewResult MyItems(CompleteQuestionnaireBrowseInputModel input)
        {
            input.ResponsibleId = _globalProvider.GetCurrentUser().Id;
            var model =
                viewRepository.Load<CompleteQuestionnaireBrowseInputModel, CompleteQuestionnaireBrowseView>(input);
            return View(model);
        }

        public ActionResult UpdateResult(string id, SurveyStatus status, UserLight responsible, string changeComment)
        {
            if (ModelState.IsValid)
            {
                commandInvoker.Execute(new UpdateCompleteQuestionnaireCommand(id,
                                                                              status,
                                                                              changeComment,
                                                                              responsible,
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
                AddAllowedStatusesToViewBag(model.Status.Id, model.Status.Name, model.Id);

            _bagManager.AddUsersToBag(ViewBag, viewRepository);
            return View(model);
        }

        [QuestionnaireAuthorize(UserRoles.Administrator, UserRoles.Supervisor, UserRoles.Operator)]
        public ActionResult Participate(string id, string mode)
        {
            var statusView = viewRepository.Load<StatusViewInputModel, StatusView>(new StatusViewInputModel(true));
            var command = new CreateNewCompleteQuestionnaireCommand(id,
                                                                    _globalProvider.GetCurrentUser(),
                                                                    new SurveyStatus(statusView.Id, statusView.Title),
                                                                    _globalProvider.GetCurrentUser());
            commandInvoker.Execute(command);


            return RedirectToAction("Question" + mode,
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
            return View(model);
        }

        [QuestionnaireAuthorize(UserRoles.Administrator, UserRoles.Supervisor, UserRoles.Operator)]
        public ViewResult QuestionV(string id, Guid? group)
        {
            if (string.IsNullOrEmpty(id))
                throw new HttpException(404, "Invalid query string parameters");
            var model =
                viewRepository.Load<CompleteQuestionnaireViewInputModel, CompleteQuestionnaireViewV>(
                    new CompleteQuestionnaireViewInputModel(id) { CurrentGroupPublicKey = group });
            ViewBag.CurrentGroup = model.CurrentGroup;

            return View(model);
        }

        [QuestionnaireAuthorize(UserRoles.Administrator, UserRoles.Supervisor, UserRoles.Operator)]
        public JsonResult Validate(string id, Guid? group, Guid? propagationKey)
        {
            commandInvoker.Execute(new ValidateGroupCommand(id, group, propagationKey, _globalProvider.GetCurrentUser()));

            var model = viewRepository.Load<CompleteQuestionnaireViewInputModel, CompleteQuestionnaireMobileView>(
                new CompleteQuestionnaireViewInputModel(id) { CurrentGroupPublicKey = group });
            return Json(model.CurrentGroup);
            //  return PartialView("~/Views/Group/_ScreenHtml5.cshtml", model);
        }

        [QuestionnaireAuthorize(UserRoles.Administrator, UserRoles.Supervisor, UserRoles.Operator)]
        public ViewResult QuestionI(string id, Guid? group)
        {
            if (string.IsNullOrEmpty(id))
                throw new HttpException(404, "Invalid query string parameters");
            var model =
                viewRepository.Load<CompleteQuestionnaireViewInputModel, CompleteQuestionnaireViewV>(
                    new CompleteQuestionnaireViewInputModel(id) { CurrentGroupPublicKey = group });
            ViewBag.CurrentGroup = model.CurrentGroup;

            return View(model);
        }

        [QuestionnaireAuthorize(UserRoles.Administrator, UserRoles.Supervisor, UserRoles.Operator)]
        public ViewResult QuestionHtml5(string id, Guid? group)
        {
            if (string.IsNullOrEmpty(id))
                throw new HttpException(404, "Invalid query string parameters");
            var model =
                viewRepository.Load<CompleteQuestionnaireViewInputModel, CompleteQuestionnaireMobileView>(
                    new CompleteQuestionnaireViewInputModel(id) { CurrentGroupPublicKey = group });
            ViewBag.CurrentGroup = model.CurrentGroup;

            return View(model);
        }

        [QuestionnaireAuthorize(UserRoles.Administrator, UserRoles.Supervisor, UserRoles.Operator)]
        public ViewResult QuestionC(string id, Guid? group)
        {
            if (string.IsNullOrEmpty(id))
                throw new HttpException(404, "Invalid query string parameters");
            var model =
                viewRepository.Load<CompleteQuestionnaireViewInputModel, CompleteQuestionnaireViewV>(
                    new CompleteQuestionnaireViewInputModel(id) { CurrentGroupPublicKey = group });
            ViewBag.CurrentGroup = model.CurrentGroup;
            ViewBag.AnsweredQuestionKey = Guid.Empty;
            ViewBag.PropogationGroupKey = Guid.Empty;
            return View(model);
        }


        public ActionResult SaveSingleResult(CompleteQuestionSettings[] settings, CompleteQuestionView[] questions)
        {
            if (questions == null || questions.Length <= 0 || !ModelState.IsValid)
            {
                //?? if it is used as prtial render on postback
                //this behaviour is wrong
                return RedirectToAction("Question", new { id = settings[0].QuestionnaireId });
            }

            var question = questions[0];
            try
            {
                commandInvoker.Execute(new UpdateAnswerInCompleteQuestionnaireCommand(settings[0].QuestionnaireId,
                                                                                      question.Answers,
                                                                                      settings[0].PropogationPublicKey,
                                                                                      _globalProvider.GetCurrentUser()));
            }
            catch (Exception e)
            {
                ModelState.AddModelError(
                    "questions[" + question.PublicKey +
                    (settings[0].PropogationPublicKey.HasValue
                         ? string.Format("_{0}", settings[0].PropogationPublicKey.Value)
                         : "") + "].AnswerValue", e.Message);
            }
            var model =
                viewRepository.Load<CompleteGroupViewInputModel, CompleteGroupView>(
                    new CompleteGroupViewInputModel(settings[0].PropogationPublicKey, settings[0].ParentGroupPublicKey,
                                                    settings[0].QuestionnaireId));
            ViewBag.CurrentGroup = model;
            return PartialView("~/Views/Group/_Screen.cshtml", model);
        }


        public ActionResult SaveSingleResultI(CompleteQuestionSettings[] settings, CompleteQuestionView[] questions,
                                              string type)
        {
            if (string.IsNullOrEmpty(type))
                type = "I";
            if (questions == null || questions.Length <= 0 || !ModelState.IsValid)
            {
                //return RedirectToAction("QuestionI", new { id = settings[0].QuestionnaireId });
                //fix wrong render on unselecting in dropdown
            }
            else
            {
                var question = questions[0];
                try
                {
                    commandInvoker.Execute(new UpdateAnswerInCompleteQuestionnaireCommand(settings[0].QuestionnaireId,
                                                                                          question.Answers,
                                                                                          settings[0].PropogationPublicKey,
                                                                                          _globalProvider.GetCurrentUser()));
                }
                catch (Exception e)
                {
                    ModelState.AddModelError("questions[" + question.PublicKey + (settings[0].PropogationPublicKey.HasValue ? string.Format("_{0}", settings[0].PropogationPublicKey.Value) : "") + "].AnswerValue", e.Message);
                }
            }

            var model = viewRepository.Load<CompleteQuestionnaireViewInputModel, CompleteQuestionnaireViewV>(
                new CompleteQuestionnaireViewInputModel(settings[0].QuestionnaireId) { CurrentGroupPublicKey = settings[0].ParentGroupPublicKey });

            return PartialView("~/Views/Group/_Screen" + type + ".cshtml", model);
        }

        public ActionResult SaveSingleResultHtml5(CompleteQuestionSettings[] settings, CompleteQuestionView[] questions, string type)
        {
            if (string.IsNullOrEmpty(type))
                type = "Html5";
            if (questions == null || questions.Length <= 0 || !ModelState.IsValid)
            {

            }
            else
            {
                var question = questions[0];
                try
                {
                    commandInvoker.Execute(new UpdateAnswerInCompleteQuestionnaireCommand(settings[0].QuestionnaireId,
                                                                                          question.Answers,
                                                                                          settings[0].PropogationPublicKey,
                                                                                          _globalProvider.GetCurrentUser()));
                }
                catch (Exception e)
                {
                    ModelState.AddModelError("questions[" + question.PublicKey + (settings[0].PropogationPublicKey.HasValue ? string.Format("_{0}", settings[0].PropogationPublicKey.Value) : "") + "].AnswerValue", e.Message);
                }
            }

            var model = viewRepository.Load<CompleteQuestionnaireViewInputModel, CompleteQuestionnaireMobileView>(
                new CompleteQuestionnaireViewInputModel(settings[0].QuestionnaireId) { CurrentGroupPublicKey = settings[0].ParentGroupPublicKey });

            return PartialView("~/Views/Group/_ScreenHtml5.cshtml", model);
        }
        public JsonResult SaveSingleResultJson(CompleteQuestionSettings[] settings, CompleteQuestionView[] questions)
        {

            var question = questions[0];
            try
            {
                commandInvoker.Execute(new UpdateAnswerInCompleteQuestionnaireCommand(settings[0].QuestionnaireId,
                                                                                      question.Answers,
                                                                                      settings[0].PropogationPublicKey,
                                                                                      _globalProvider.GetCurrentUser()));
            }
            catch (Exception e)
            {
                return Json(new {question = questions[0], error = e.Message});
            }


            var model = viewRepository.Load<CompleteQuestionnaireViewInputModel, CompleteQuestionnaireMobileView>(
                new CompleteQuestionnaireViewInputModel(settings[0].QuestionnaireId)
                    {CurrentGroupPublicKey = settings[0].ParentGroupPublicKey});

            return Json(model.CurrentGroup);
        }

        public ActionResult SaveSingleResultV(CompleteQuestionSettings[] settings, CompleteQuestionView[] questions)
        {
            if (questions == null || questions.Length <= 0 || !ModelState.IsValid)
            {
                return RedirectToAction("QuestionV", new { id = settings[0].QuestionnaireId });
            }

            var question = questions[0];
            try
            {
                commandInvoker.Execute(new UpdateAnswerInCompleteQuestionnaireCommand(settings[0].QuestionnaireId,
                                                                                      question.Answers,
                                                                                      settings[0].PropogationPublicKey,
                                                                                      _globalProvider.GetCurrentUser()));
            }
            catch (Exception e)
            {
                ModelState.AddModelError("questions[" + question.PublicKey + (settings[0].PropogationPublicKey.HasValue
                                                                                  ? string.Format("_{0}",
                                                                                                  settings[0].
                                                                                                      PropogationPublicKey
                                                                                                      .Value)
                                                                                  : "") + "].AnswerValue", e.Message);
            }

            var model = viewRepository.Load<CompleteQuestionnaireViewInputModel, CompleteQuestionnaireViewV>(
                new CompleteQuestionnaireViewInputModel(settings[0].QuestionnaireId) { CurrentGroupPublicKey = settings[0].ParentGroupPublicKey });

            return PartialView("~/Views/Group/_ScreenV.cshtml", model);
        }

        public ActionResult SaveSingleResultC(CompleteQuestionSettings[] settings, CompleteQuestionView[] questions)
        {
            if (questions == null || questions.Length <= 0 || !ModelState.IsValid)
            {
                return RedirectToAction("QuestionC", new { id = settings[0].QuestionnaireId });
            }

            var question = questions[0];

            try
            {
                commandInvoker.Execute(new UpdateAnswerInCompleteQuestionnaireCommand(settings[0].QuestionnaireId,
                                                                                      question.Answers,
                                                                                      settings[0].PropogationPublicKey,
                                                                                      _globalProvider.GetCurrentUser()));
            }
            catch (Exception e)
            {
                ModelState.AddModelError("questions[" + question.PublicKey + (settings[0].PropogationPublicKey.HasValue
                                                                                  ? string.Format("_{0}",
                                                                                                  settings[0].
                                                                                                      PropogationPublicKey
                                                                                                      .Value)
                                                                                  : "") + "].AnswerValue", e.Message);
            }

            var model = viewRepository.Load<CompleteQuestionnaireViewInputModel, CompleteQuestionnaireViewV>(
                new CompleteQuestionnaireViewInputModel(settings[0].QuestionnaireId) { CurrentGroupPublicKey = settings[0].ParentGroupPublicKey });

            ViewBag.AnsweredQuestionKey = question.PublicKey;
            ViewBag.PropogationGroupKey = settings[0].PropogationPublicKey;

            return PartialView("~/Views/Group/_ScreenC.cshtml", model);
        }


        public ActionResult Delete(string id)
        {
            commandInvoker.Execute(new DeleteCompleteQuestionnaireCommand(id, _globalProvider.GetCurrentUser()));
            return RedirectToAction("Index");
        }

        protected void AddAllowedStatusesToViewBag(string statusId, string statusName, string Qid)
        {
            var statuses = new List<SurveyStatus>();

            var isCurrentPresent = false;


            var rawStatuses = viewRepository.Load<StatusBrowseInputModel, StatusBrowseView>(new StatusBrowseInputModel
                     {
                         PageSize = 300,
                         QId = Qid
                     });
            var model1 = rawStatuses == null ? new List<StatusBrowseItem>() : rawStatuses.Items;

            var model = viewRepository.Load<StatusViewInputModel, StatusView>(new StatusViewInputModel(statusId));
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