#region

using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using Questionnaire.Core.Web.Helpers;
using Questionnaire.Core.Web.Security;
using RavenQuestionnaire.Core;
using RavenQuestionnaire.Core.Commands.Questionnaire.Completed;
using RavenQuestionnaire.Core.Commands.Questionnaire.Group;
using RavenQuestionnaire.Core.Commands.Statistics;
using RavenQuestionnaire.Core.Entities.SubEntities;
using RavenQuestionnaire.Core.Utility;
using RavenQuestionnaire.Core.Views.CompleteQuestionnaire;
using RavenQuestionnaire.Core.Views.CompleteQuestionnaire.Mobile;
using RavenQuestionnaire.Core.Views.CompleteQuestionnaire.Vertical;
using RavenQuestionnaire.Core.Views.Group;
using RavenQuestionnaire.Core.Views.Question;
using RavenQuestionnaire.Core.Views.Status;
using RavenQuestionnaire.Core.Views.Status.StatusElement;
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

        public ActionResult UpdateResult(string id, SurveyStatus Status, UserLight responsible, string StatusHolderId)
        {
            if (ModelState.IsValid)
            {
                commandInvoker.Execute(new UpdateCompleteQuestionnaireCommand(id,
                                                                              Status.PublicId,
                                                                              StatusHolderId,
                                                                              IdUtil.CreateUserId(responsible.Id),
                                                                              _globalProvider.GetCurrentUser()));
            }
            return RedirectToAction("Index");
        }



        public ActionResult ReInit(string id)
        {
            if (string.IsNullOrEmpty(id))
                throw new HttpException(404, "Invalid query string parameters");

            var model = viewRepository.Load<CompleteQuestionnaireViewInputModel,
                CompleteQuestionnaireView>(new CompleteQuestionnaireViewInputModel(id));

            if (model != null)
            {
                var status = viewRepository.Load<StatusViewInputModel, StatusView>(new StatusViewInputModel(IdUtil.ParseId(model.TemplateId)));
                if (status != null)
                {
                    var statusItem = status.StatusElements.FirstOrDefault(x => x.IsInitial == true);//temporary hardcoded
                    if (statusItem != null)
                    {
                        commandInvoker.Execute(new UpdateCompleteQuestionnaireCommand(id,
                                                                                          statusItem.PublicKey,
                                                                                          status.Id,
                                                                                          null,
                                                                                          _globalProvider.GetCurrentUser()));

                    }
                }

            }

            return RedirectToAction("QuestionHtml5", "CompleteQuestionnaire", new { id = id });
             
        }

        public ActionResult Complete(string id)
        {
            if (string.IsNullOrEmpty(id))
                throw new HttpException(404, "Invalid query string parameters");

            var model = viewRepository.Load<CompleteQuestionnaireViewInputModel,
                CompleteQuestionnaireView>(new CompleteQuestionnaireViewInputModel(id));

            if (model != null)
            {
                commandInvoker.Execute(new ValidateGroupCommand(id, null, null, _globalProvider.GetCurrentUser()));

                var modelChecked = viewRepository.Load<CompleteQuestionnaireViewInputModel,
                    CompleteQuestionnaireView>(new CompleteQuestionnaireViewInputModel(id));

                if (modelChecked != null )
                {
                    var status = viewRepository.Load<StatusViewInputModel, StatusView>(new StatusViewInputModel(IdUtil.ParseId(modelChecked.TemplateId)));

                    if (status != null)
                    {
                        if (modelChecked.IsValid)
                        {
                            var statusItem = status.StatusElements.FirstOrDefault(x => x.Title == "Completed");//temporary hardcoded

                            commandInvoker.Execute(new UpdateCompleteQuestionnaireCommand(id,
                                                                                          statusItem.PublicKey,
                                                                                          status.Id,
                                                                                          null,
                                                                                          _globalProvider.GetCurrentUser()));

                       
                            return RedirectToAction("Index", "Dashboard");
                        }
                        else
                        {
                            var statusItem = status.StatusElements.FirstOrDefault(x => x.Title == "Error");//temporary hardcoded
                            commandInvoker.Execute(new UpdateCompleteQuestionnaireCommand(id,
                                                                                          statusItem.PublicKey,
                                                                                          status.Id,
                                                                                          null,
                                                                                          _globalProvider.GetCurrentUser()));


                           return Redirect(Url.RouteUrl(new { controller = "Statistic", action = "Details", id = id }) + "#" + "invalid");
                           // return RedirectToAction("Details", "Statistic", new {id = id});
                        }

                        
                    }
                }
            }

            return RedirectToAction("Index", "Dashboard");
            
        }

        public ViewResult Result(string id)
        {
            if (string.IsNullOrEmpty(id))
                throw new HttpException(404, "Invalid query string parameters");
            var model = viewRepository.Load<CompleteQuestionnaireViewInputModel,
                CompleteQuestionnaireView>(new CompleteQuestionnaireViewInputModel(id));

            if (model != null)
            {
                string Qid = IdUtil.ParseId(model.TemplateId); //TODO: avoid parse and then build
                var status = viewRepository.Load<StatusViewInputModel, StatusView>(new StatusViewInputModel(Qid));

                if (status != null)
                {
                    ViewBag.StatusHolderId = status.Id;
                    AddAllowedStatusesToViewBag(model.Status.PublicId, model.Status.Name, Qid, status);
                }
            }
           

            _bagManager.AddUsersToBag(ViewBag, viewRepository);
            return View(model);
        }

        // move out of there!!
        private SurveyStatus GetStatus(string id)
        {
            var statusView = viewRepository.Load<StatusItemViewInputModel, StatusItemView>(new StatusItemViewInputModel(id, true));

            SurveyStatus status = new SurveyStatus();

            if (statusView == null)
            {
                status.PublicId = new Guid("{A90E95AC-95E7-4ADC-B070-FDE36952769B}");
                status.Name = "[Unknown]";
            }

            else
            {
                status.PublicId = statusView.PublicKey;
                status.Name = statusView.Title;
            }
            return status;
        }

        [QuestionnaireAuthorize(UserRoles.Administrator, UserRoles.Supervisor, UserRoles.Operator)]
        public ActionResult Participate(string id, string mode)
        {
            SurveyStatus status = GetStatus(id);
            var questionnairePublicKey = Guid.NewGuid();
            var command = new CreateNewCompleteQuestionnaireCommand(id,questionnairePublicKey,
                                                                    _globalProvider.GetCurrentUser(),
                                                                   status,
                                                                    _globalProvider.GetCurrentUser());
            commandInvoker.Execute(command);

            return RedirectToAction("Question" + mode,
                                    new
                                        {
                                            id = questionnairePublicKey
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
            return Json(model);
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
        public ViewResult QuestionHtml5(string id, Guid? group, Guid? question)
        {
            if (string.IsNullOrEmpty(id))
                throw new HttpException(404, "Invalid query string parameters");
            var model =
                viewRepository.Load<CompleteQuestionnaireViewInputModel, CompleteQuestionnaireMobileView>(
                    new CompleteQuestionnaireViewInputModel(id) { CurrentGroupPublicKey = group });
            //ViewBag.CurrentGroup = model.CurrentGroup;
            ViewBag.CurrentQuestion = question.HasValue ? question.Value : new Guid();
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

        public JsonResult SaveSingleResultJson(CompleteQuestionSettings[] settings, CompleteQuestionView[] questions)
        {

            var question = questions[0];
            try
            {
                commandInvoker.Execute(new UpdateAnswerInCompleteQuestionnaireCommand(settings[0].QuestionnaireId,
                                                                                      question,
                                                                                      settings[0].PropogationPublicKey,
                                                                                      _globalProvider.GetCurrentUser()));
            }
            catch (Exception e)
            {
                return Json(new {question = questions[0],settings=settings[0], error = e.Message});
            }


            var model = viewRepository.Load<CompleteQuestionnaireViewInputModel, CompleteQuestionnaireMobileView>(
                new CompleteQuestionnaireViewInputModel(settings[0].QuestionnaireId)
                    {CurrentGroupPublicKey = settings[0].ParentGroupPublicKey});

            return Json(model);
        }



        public ActionResult Delete(string id)
        {
            commandInvoker.Execute(new DeleteCompleteQuestionnaireCommand(id, _globalProvider.GetCurrentUser()));
            return RedirectToAction("Index","Dashboard");
        }


        protected void AddAllowedStatusesToViewBag(Guid publicKey, string statusName, string Qid, StatusView statusView)
        {
            var statuses = new List<SurveyStatus>();
            var isCurrentPresent = false;
            
            var status = statusView.StatusElements.FirstOrDefault(x => x.PublicKey == publicKey);
            if (status != null)
                {

                    foreach (var role in Roles.GetRolesForUser())
                    {
                        if (status.StatusRoles.ContainsKey(role))
                            foreach (var item in status.StatusRoles[role])
                            {
                                if (statuses.Contains(item)) continue;
                                statuses.Add(item);
                                if (isCurrentPresent) continue;

                                if (item.PublicId == publicKey)
                                    isCurrentPresent = true;
                            }
                    }
                }
            

            if (!isCurrentPresent)
                statuses.Add(new SurveyStatus(publicKey, statusName));

            ViewBag.AvailableStatuses = statuses;
        }
    }
}