#region

using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using Ncqrs;
using Ncqrs.Commanding.ServiceModel;
using Questionnaire.Core.Web.Helpers;
using Questionnaire.Core.Web.Security;
using RavenQuestionnaire.Core;
using RavenQuestionnaire.Core.Commands.Questionnaire.Completed;
using RavenQuestionnaire.Core.Commands.Questionnaire.Group;
using RavenQuestionnaire.Core.Entities.SubEntities;
using RavenQuestionnaire.Core.Utility;
using RavenQuestionnaire.Core.Views.CompleteQuestionnaire;
using RavenQuestionnaire.Core.Views.CompleteQuestionnaire.Grouped;
using RavenQuestionnaire.Core.Views.CompleteQuestionnaire.Json;
using RavenQuestionnaire.Core.Views.CompleteQuestionnaire.Mobile;
using RavenQuestionnaire.Core.Views.Question;
using RavenQuestionnaire.Core.Views.Statistics;
using RavenQuestionnaire.Core.Views.Status;
using RavenQuestionnaire.Core.Views.Status.StatusElement;
using Web.CAPI.Models;

#endregion

namespace Web.CAPI.Controllers
{
    [Authorize]
    public class SurveyController : Controller
    {
        private readonly IGlobalInfoProvider _globalProvider;
       // private readonly ICommandInvoker commandInvoker;
        private readonly IViewRepository viewRepository;

        public SurveyController(ICommandInvoker commandInvoker, IViewRepository viewRepository, IGlobalInfoProvider globalProvider)
        {
         //   this.commandInvoker = commandInvoker;
            this.viewRepository = viewRepository;
            _globalProvider = globalProvider;
        }

        public ViewResult Index(string id, Guid? group, Guid? question)
        {
            if (string.IsNullOrEmpty(id))
                throw new HttpException(404, "Invalid query string parameters");
            var model =
                viewRepository.Load<CompleteQuestionnaireViewInputModel, CompleteQuestionnaireMobileView>(
                    new CompleteQuestionnaireViewInputModel(id) { CurrentGroupPublicKey = group });
            ViewBag.CurrentQuestion = question.HasValue ? question.Value : new Guid();
            return View(model);
        }
        public PartialViewResult Screen(string id, Guid group, Guid? propagationKey)
        {
            if (string.IsNullOrEmpty(id))
                throw new HttpException(404, "Invalid query string parameters");
            var model = viewRepository.Load<CompleteQuestionnaireViewInputModel, CompleteGroupMobileView>(
               new CompleteQuestionnaireViewInputModel(id, group,propagationKey));
            ViewBag.CurrentQuestion =  new Guid();
            return PartialView("_SurveyContent",model);
        }

        [HttpPost]
        public PartialViewResult _SurveyContent(string id, Guid? group, Guid? question)
        {
            if (string.IsNullOrEmpty(id))
                throw new HttpException(404, "Invalid query string parameters");
            var model = viewRepository.Load<CompleteQuestionnaireViewInputModel, CompleteQuestionnaireMobileView>(
                    new CompleteQuestionnaireViewInputModel(id) { CurrentGroupPublicKey = group });
            ViewBag.CurrentQuestion = question.HasValue ? question.Value : new Guid();
            return PartialView("_SurveyContent", model);
        }

        public ActionResult Statistic(string id)
        {
            var stat = viewRepository.Load<CompleteQuestionnaireStatisticViewInputModel, CompleteQuestionnaireStatisticView>(new CompleteQuestionnaireStatisticViewInputModel(id));
            return View(stat);
        }

        public ActionResult Dashboard()
        {
            var model = viewRepository.Load<CQGroupedBrowseInputModel, CQGroupedBrowseView>(new CQGroupedBrowseInputModel());
            return View(model);
        }
        
        public JsonResult SaveComments(CompleteQuestionSettings[] settings, CompleteQuestionView[] questions)
        {
            var question = questions[0];
            question.PublicKey = new Guid(Request.Form["PublicKey"]);
            try
            {
                var commandService = NcqrsEnvironment.Get<ICommandService>();
                Guid questionnaireKey = Guid.Parse(settings[0].QuestionnaireId);
                commandService.Execute(new SetCommentCommand(questionnaireKey, question, settings[0].PropogationPublicKey));
           /*     commandInvoker.Execute(new UpdateCommentsInCompleteQuestionnaireCommand(settings[0].QuestionnaireId,
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


        public ActionResult Complete(string id)
        {
            if (string.IsNullOrEmpty(id))
                throw new HttpException(404, "Invalid query string parameters");

            Guid key = new Guid();
            if (!Guid.TryParse(id, out key))
                throw new HttpException(404, "Invalid query string parameters");

            var commandService = NcqrsEnvironment.Get<ICommandService>();
            commandService.Execute(new ChangeStatusCommand() { CompleteQuestionnaireId = key, Status = SurveyStatus.Complete});

            /*var model = viewRepository.Load<CompleteQuestionnaireViewInputModel,
                CompleteQuestionnaireView>(new CompleteQuestionnaireViewInputModel(id));

            if (model != null)
            {
                commandInvoker.Execute(new ValidateGroupCommand(id, null, null, _globalProvider.GetCurrentUser()));

                var modelChecked = viewRepository.Load<CompleteQuestionnaireViewInputModel,
                    CompleteQuestionnaireView>(new CompleteQuestionnaireViewInputModel(id));

                if (modelChecked != null)
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


                            return RedirectToAction("Dashboard", "Survey");
                        }
                        else
                        {
                            var statusItem = status.StatusElements.FirstOrDefault(x => x.Title == "Error");//temporary hardcoded
                            commandInvoker.Execute(new UpdateCompleteQuestionnaireCommand(id,
                                                                                          statusItem.PublicKey,
                                                                                          status.Id,
                                                                                          null,
                                                                                          _globalProvider.GetCurrentUser()));


                            return Redirect(Url.RouteUrl(new { controller = "Survey", action = "Statistic", id = id }) + "#" + "invalid");
                        }
                    }
                }
            }*/

            return RedirectToAction("Dashboard", "Survey");

        }

        public ActionResult ReInit(string id)
        {
            var commandService = NcqrsEnvironment.Get<ICommandService>();
            commandService.Execute(new ChangeStatusCommand(){CompleteQuestionnaireId = Guid.Parse(id), Status = SurveyStatus.Initial});

            /* if (string.IsNullOrEmpty(id))
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

             }*/
            return RedirectToAction("Index", "Survey", new { id = id });
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
                //  return RedirectToAction("Index", "Dashboard");
                throw new HttpException("404");


            var newQuestionnairePublicKey = Guid.NewGuid();
          
            var commandService = NcqrsEnvironment.Get<ICommandService>();
            commandService.Execute(new CreateCompleteQuestionnaireCommand(newQuestionnairePublicKey, key));


            return RedirectToAction("Index", new { id = newQuestionnairePublicKey });
        }



     /*   [QuestionnaireAuthorize(UserRoles.Administrator, UserRoles.Supervisor, UserRoles.Operator)]
        public JsonResult Validate(string id, Guid? group, Guid? propagationKey)
        {
            commandInvoker.Execute(new ValidateGroupCommand(id, group, propagationKey, _globalProvider.GetCurrentUser()));

            var model = viewRepository.Load<CompleteQuestionnaireViewInputModel, CompleteQuestionnaireMobileView>(
                new CompleteQuestionnaireViewInputModel(id) { CurrentGroupPublicKey = group });
            return Json(model);
        }*/

        public JsonResult SaveAnswer(CompleteQuestionSettings[] settings, CompleteQuestionView[] questions)
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
                new CompleteQuestionnaireViewInputModel(settings[0].QuestionnaireId, settings[0].ParentGroupPublicKey.Value,settings[0].PropogationPublicKey));

            return Json(model);
        }


        public JsonResult PropagateGroup(Guid publicKey, Guid parentGroupPublicKey, string questionnaireId)
        {
            try
            {
                var propagationKey = Guid.NewGuid();
                var commandService = NcqrsEnvironment.Get<ICommandService>();
                commandService.Execute(new AddPropagatableGroupCommand(Guid.Parse(questionnaireId), propagationKey, publicKey));


                var model = viewRepository.Load<CompleteQuestionnaireViewInputModel, CompleteQuestionnaireJsonView>(
                new CompleteQuestionnaireViewInputModel(questionnaireId) { CurrentGroupPublicKey = parentGroupPublicKey });
                return Json(new { propagationKey = propagationKey, parentGroupPublicKey = publicKey, group = model });
            }
            catch (Exception e)
            {
                ModelState.AddModelError("PropagationError", e.Message);
                return Json(new { error = e.Message, parentGroupPublicKey = publicKey });
            }
        }

        public JsonResult DeletePropagatedGroup(Guid propagationKey, Guid publicKey, Guid parentGroupPublicKey,
                                                  string questionnaireId)
        {
            var commandService = NcqrsEnvironment.Get<ICommandService>();
            commandService.Execute(new DeletePropagatableGroupCommand(Guid.Parse(questionnaireId), propagationKey, publicKey));

            return Json(new { propagationKey = propagationKey });
        }

        public ActionResult Delete(string id)
        {
            var service = NcqrsEnvironment.Get<ICommandService>();
            service.Execute(new DeleteCompleteQuestionnaireCommand(Guid.Parse(id)));
            return RedirectToAction("Dashboard", "Survey");
        }
    }
}