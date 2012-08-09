#region Assembly

using Ncqrs;
using System;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using RavenQuestionnaire.Core;
using System.Collections.Generic;
using Ncqrs.Commanding.ServiceModel;
using RavenQuestionnaire.Web.Models;
using Questionnaire.Core.Web.Helpers;
using Questionnaire.Core.Web.Security;
using RavenQuestionnaire.Core.Utility;
using RavenQuestionnaire.Core.Views.Status;
using RavenQuestionnaire.Core.Views.Question;
using RavenQuestionnaire.Core.Views.StatusReport;
using RavenQuestionnaire.Core.Entities.SubEntities;
using RavenQuestionnaire.Core.Views.Status.StatusElement;
using RavenQuestionnaire.Core.Views.CompleteQuestionnaire;
using RavenQuestionnaire.Core.Commands.Questionnaire.Group;
using RavenQuestionnaire.Core.Commands.Questionnaire.Completed;
using RavenQuestionnaire.Core.Views.CompleteQuestionnaire.Json;
using RavenQuestionnaire.Core.Views.CompleteQuestionnaire.Mobile;
using RavenQuestionnaire.Core.Views.CompleteQuestionnaire.Vertical;

#endregion

namespace RavenQuestionnaire.Web.Controllers
{
    [Authorize]
    public class CompleteQuestionnaireController : Controller
    {
        #region Properties

        private readonly IBagManager _bagManager;
        private readonly IGlobalInfoProvider _globalProvider;
        private readonly IViewRepository viewRepository;

        #endregion

        #region Constructor

        public CompleteQuestionnaireController( IViewRepository viewRepository,
                                               IBagManager bagManager, IGlobalInfoProvider globalProvider)
        {
            this.viewRepository = viewRepository;
            _bagManager = bagManager;
            _globalProvider = globalProvider;
        }

        #endregion

        #region Actions

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
               /* commandInvoker.Execute(new UpdateCompleteQuestionnaireCommand(id,
                                                                              Status.PublicId,
                                                                              StatusHolderId,
                                                                              IdUtil.CreateUserId(responsible.Id),
                                                                              _globalProvider.GetCurrentUser()));*/
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
                        /*commandInvoker.Execute(new UpdateCompleteQuestionnaireCommand(id,
                                                                                          statusItem.PublicKey,
                                                                                          status.Id,
                                                                                          null,
                                                                                          _globalProvider.GetCurrentUser()));*/

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
                //commandInvoker.Execute(new ValidateGroupCommand(id, null, null, _globalProvider.GetCurrentUser()));
                var modelChecked = viewRepository.Load<CompleteQuestionnaireViewInputModel,
                    CompleteQuestionnaireView>(new CompleteQuestionnaireViewInputModel(id));
                if (modelChecked != null)
                {
                    var status = viewRepository.Load<StatusViewInputModel, StatusView>(new StatusViewInputModel(IdUtil.ParseId(modelChecked.TemplateId)));
                    if (status != null){
                        if (modelChecked.IsValid)
                        {
                            var statusItem = status.StatusElements.FirstOrDefault(x => x.Title == "Completed");//temporary hardcoded
                           /* commandInvoker.Execute(new UpdateCompleteQuestionnaireCommand(id,
                                                                                          statusItem.PublicKey,
                                                                                          status.Id,
                                                                                          null,
                                                                                          _globalProvider.GetCurrentUser()));*/

                            return RedirectToAction("Index", "Dashboard");
                        }
                        else
                        {
                            var statusItem = status.StatusElements.FirstOrDefault(x => x.Title == "Error");//temporary hardcoded
                            /*commandInvoker.Execute(new UpdateCompleteQuestionnaireCommand(id,
                                                                                          statusItem.PublicKey,
                                                                                          status.Id,
                                                                                          null,
                                                                                          _globalProvider.GetCurrentUser()));*/


                            return Redirect(Url.RouteUrl(new { controller = "Statistic", action = "Details", id = id }) + "#" + "invalid");
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
            Guid key;
            if (!Guid.TryParse(id, out key))
                throw  new HttpException("404");
            var newQuestionnairePublicKey = Guid.NewGuid();
            var commandService = NcqrsEnvironment.Get<ICommandService>();
            commandService.Execute(new CreateCompleteQuestionnaireCommand(newQuestionnairePublicKey, key));
            return RedirectToAction("Question" + mode,
                                    new
                                        {
                                            id = newQuestionnairePublicKey
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
            //commandInvoker.Execute(new ValidateGroupCommand(id, group, propagationKey, _globalProvider.GetCurrentUser()));

            var model = viewRepository.Load<CompleteQuestionnaireViewInputModel, CompleteQuestionnaireMobileView>(
                new CompleteQuestionnaireViewInputModel(id) { CurrentGroupPublicKey = group });
            return Json(model);
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


        public JsonResult SaveCommentsJson(CompleteQuestionSettings[] settings, CompleteQuestionView[] questions, string PublicKey)
        {
            var question = questions[0];
            question.PublicKey = new Guid(Request.Form["PublicKey"]);
            try
            {
                /*commandInvoker.Execute(new UpdateCommentsInCompleteQuestionnaireCommand(settings[0].QuestionnaireId,
                                                                                      question,
                                                                                      settings[0].PropogationPublicKey,
                                                                                      _globalProvider.GetCurrentUser()));*/
            }
            catch (Exception e)
            {
                return Json(new { question = questions[0], settings = settings[0], error = e.Message });
            }
            var model = viewRepository.Load<CompleteQuestionnaireViewInputModel, CompleteQuestionnaireJsonView>(
                new CompleteQuestionnaireViewInputModel(settings[0].QuestionnaireId) { CurrentGroupPublicKey = settings[0].ParentGroupPublicKey });
            return Json(model);
        }


        public JsonResult SaveSingleResultJson(CompleteQuestionSettings[] settings, CompleteQuestionView[] questions)
        {

            var question = questions[0];
            try
            {
                var commandService = NcqrsEnvironment.Get<ICommandService>();
                commandService.Execute(new SetAnswerCommand(Guid.Parse(settings[0].QuestionnaireId), question, 
                    settings[0].PropogationPublicKey));
                

            }
            catch (Exception e)
            {
                return Json(new { question = questions[0], settings = settings[0], error = e.Message });
            }


            var model = viewRepository.Load<CompleteQuestionnaireViewInputModel, CompleteQuestionnaireJsonView>(
                new CompleteQuestionnaireViewInputModel(settings[0].QuestionnaireId) { CurrentGroupPublicKey = settings[0].ParentGroupPublicKey });

            return Json(model);
        }
        
        public ActionResult Delete(string id)
        {
            var service = NcqrsEnvironment.Get<ICommandService>();
            service.Execute(new DeleteCompleteQuestionnaireCommand(Guid.Parse(id)));
            return RedirectToAction("Index", "Dashboard");
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

        public ActionResult QStatuses(string questionnaireId, Guid statusId)
        {
            var model = viewRepository.Load<CQStatusReportViewInputModel, CQStatusReportView>(new CQStatusReportViewInputModel(questionnaireId, statusId));
            return View(model);
        }

        #endregion
    }
}