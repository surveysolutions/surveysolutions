using Ncqrs;
using System;
using System.Web;
using System.Web.Mvc;
using Web.CAPI.Models;
using RavenQuestionnaire.Core;
using Ncqrs.Commanding.ServiceModel;
using Questionnaire.Core.Web.Helpers;
using Questionnaire.Core.Web.Security;
using RavenQuestionnaire.Core.Views.Question;
using RavenQuestionnaire.Core.Views.Statistics;
using RavenQuestionnaire.Core.Entities.SubEntities;
using RavenQuestionnaire.Core.Views.CompleteQuestionnaire;
using RavenQuestionnaire.Core.Commands.Questionnaire.Group;
using RavenQuestionnaire.Core.Views.CompleteQuestionnaire.Json;
using RavenQuestionnaire.Core.Commands.Questionnaire.Completed;
using RavenQuestionnaire.Core.Views.CompleteQuestionnaire.Grouped;
using RavenQuestionnaire.Core.Views.CompleteQuestionnaire.Mobile;

namespace Web.CAPI.Controllers
{
    [Authorize]
    public class SurveyController : Controller
    {
        #region Properties

        private readonly IGlobalInfoProvider _globalProvider;
        private readonly IViewRepository viewRepository;

        #endregion

        #region Constructor

        public SurveyController(IViewRepository viewRepository, IGlobalInfoProvider globalProvider)
        {
            this.viewRepository = viewRepository;
            this._globalProvider = globalProvider;
        }

        #endregion

        #region Actions 

        public ViewResult Index(Guid id, Guid? group, Guid? question, Guid? propagationKey)
        {
            if (id == Guid.Empty)
                throw new HttpException(404, "Invalid query string parameters");
            var model = viewRepository.Load<CompleteQuestionnaireViewInputModel, CompleteQuestionnaireMobileView>(
                new CompleteQuestionnaireViewInputModel(id)
                    {CurrentGroupPublicKey = group, PropagationKey = propagationKey});
            ViewBag.CurrentQuestion = question.HasValue ? question.Value : new Guid();
            ViewBag.PagePrefix = "page-to-delete";
            return View(model);
        }

        public PartialViewResult Screen(Guid id, Guid group, Guid? propagationKey)
        {
            if (id == Guid.Empty)
                throw new HttpException(404, "Invalid query string parameters");
            var model = viewRepository.Load<CompleteQuestionnaireViewInputModel, CompleteGroupMobileView>(
                new CompleteQuestionnaireViewInputModel(id, group, propagationKey));
            ViewBag.CurrentQuestion = new Guid();
            ViewBag.PagePrefix = "";
            return PartialView("_SurveyContent", model);
        }
        public PartialViewResult CompleteSummary(Guid id)
        {
            if (id == Guid.Empty)
                throw new HttpException(404, "Invalid query string parameters");
            var stat = viewRepository.Load
                <CompleteQuestionnaireStatisticViewInputModel, CompleteQuestionnaireStatisticView>(
                    new CompleteQuestionnaireStatisticViewInputModel(id.ToString()));
            return PartialView("Complete/_Main", stat);
        }
        public PartialViewResult Answered(Guid id)
        {
            if (id == Guid.Empty)
                throw new HttpException(404, "Invalid query string parameters");
            var stat = viewRepository.Load
                <CompleteQuestionnaireStatisticViewInputModel, CompleteQuestionnaireStatisticView>(
                    new CompleteQuestionnaireStatisticViewInputModel(id.ToString()));
            return PartialView("Complete/_Answered", stat);
        }
        public PartialViewResult Unanswered(Guid id)
        {
            if (id == Guid.Empty)
                throw new HttpException(404, "Invalid query string parameters");
            var stat = viewRepository.Load
                <CompleteQuestionnaireStatisticViewInputModel, CompleteQuestionnaireStatisticView>(
                    new CompleteQuestionnaireStatisticViewInputModel(id.ToString()));
            return PartialView("Complete/_Unanswered", stat);
        }
        public PartialViewResult Invalid(Guid id)
        {
            if (id == Guid.Empty)
                throw new HttpException(404, "Invalid query string parameters");
            var stat = viewRepository.Load
                <CompleteQuestionnaireStatisticViewInputModel, CompleteQuestionnaireStatisticView>(
                    new CompleteQuestionnaireStatisticViewInputModel(id.ToString()));
            return PartialView("Complete/_Invalid", stat);
        }
        [HttpPost]
        public PartialViewResult _SurveyContent(Guid id, Guid? group, Guid? question)
        {
            if (Guid.Empty == id)
                throw new HttpException(404, "Invalid query string parameters");
            var model = viewRepository.Load<CompleteQuestionnaireViewInputModel, CompleteQuestionnaireMobileView>(
                new CompleteQuestionnaireViewInputModel(id) {CurrentGroupPublicKey = group});
            ViewBag.CurrentQuestion = question.HasValue ? question.Value : new Guid();
            return PartialView("_SurveyContent", model);
        }

        public ActionResult Statistic(string id)
        {
            var stat = viewRepository.Load<CompleteQuestionnaireStatisticViewInputModel, CompleteQuestionnaireStatisticView>(
                    new CompleteQuestionnaireStatisticViewInputModel(id));
            return View(stat);
        }

        public ViewResult Dashboard()
        {
            var user = _globalProvider.GetCurrentUser();
            var inputModel = new CQGroupedBrowseInputModel();
            if(user!=null)
            inputModel.InterviewerId = Guid.Parse(user.Id);
            var model =
                viewRepository.Load<CQGroupedBrowseInputModel, CQGroupedBrowseView>(inputModel);
            return View(model);
        }

        public JsonResult SaveComments(CompleteQuestionSettings[] settings, CompleteQuestionView[] questions)
        {
            var question = questions[0];
            question.PublicKey = new Guid(Request.Form["PublicKey"]);
            try
            {
                var commandService = NcqrsEnvironment.Get<ICommandService>();
                Guid questionnaireKey = settings[0].QuestionnaireId;
                commandService.Execute(new SetCommentCommand(questionnaireKey, question,
                                                             settings[0].PropogationPublicKey));
            }
            catch (Exception e)
            {
                NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();
                logger.Fatal(e);
                return Json(new {question = questions[0], settings = settings[0], error = e.Message});
            }
            var model = viewRepository.Load<CompleteQuestionnaireViewInputModel, CompleteQuestionnaireJsonView>(
                new CompleteQuestionnaireViewInputModel(settings[0].QuestionnaireId)
                    {CurrentGroupPublicKey = settings[0].ParentGroupPublicKey});
            return Json(model);
        }


        public ActionResult Complete(string id, string comments)
        {
            if (string.IsNullOrEmpty(id))
                throw new HttpException(404, "Invalid query string parameters");
            Guid key = new Guid();
            if (!Guid.TryParse(id, out key))
                throw new HttpException(404, "Invalid query string parameters");
            var stat = viewRepository.Load
                <CompleteQuestionnaireStatisticViewInputModel, CompleteQuestionnaireStatisticView>
                (new CompleteQuestionnaireStatisticViewInputModel(id));
            
            var commandService = NcqrsEnvironment.Get<ICommandService>();
            SurveyStatus status;
            if (stat != null && stat.InvalidQuestions.Count > 0)
            {
                status = SurveyStatus.Error;
                status.ChangeComment = comments;
                commandService.Execute(new ChangeStatusCommand() { CompleteQuestionnaireId = key, Status = status });
                //return Redirect(Url.RouteUrl(new {controller = "Survey", action = "Statistic", id = id}) + "#" + "invalid");
                // return Json(new {message = "Error"}, JsonRequestBehavior.AllowGet);

                return RedirectToAction("Index",
                                        new
                                            {
                                                id = id,
                                                group = stat.InvalidQuestions[0].GroupPublicKey,
                                                question = stat.InvalidQuestions[0].PublicKey,
                                                propagationKey = stat.InvalidQuestions[0].GroupPropagationPublicKey
                                            });
            }
            status = SurveyStatus.Complete;
            status.ChangeComment = comments;
            commandService.Execute(new ChangeStatusCommand()
                                       {CompleteQuestionnaireId = key, Status = status});
            return RedirectToAction("Dashboard");
        }

        public ActionResult ReInit(string id)
        {
            var commandService = NcqrsEnvironment.Get<ICommandService>();
            commandService.Execute(new ChangeStatusCommand()
                                       {CompleteQuestionnaireId = Guid.Parse(id), Status = SurveyStatus.Initial});
            return RedirectToAction("Index", "Survey", new {id = id});
        }

        // move out of there!!
        /*private SurveyStatus GetStatus(string id)
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
        }*/

        [QuestionnaireAuthorize(UserRoles.Administrator, UserRoles.Supervisor, UserRoles.Operator)]
        public ActionResult Participate(string id)
        {
            Guid key;
            if (!Guid.TryParse(id, out key))
                throw new HttpException("404");
            var newQuestionnairePublicKey = Guid.NewGuid();
            var commandService = NcqrsEnvironment.Get<ICommandService>();
            commandService.Execute(new CreateCompleteQuestionnaireCommand(newQuestionnairePublicKey, key));
            return RedirectToAction("Index", new {id = newQuestionnairePublicKey});
        }

        public JsonResult SaveAnswer(CompleteQuestionSettings[] settings, CompleteQuestionView[] questions)
        {
            var question = questions[0];
            try
            {
                var commandService = NcqrsEnvironment.Get<ICommandService>();
                commandService.Execute(new SetAnswerCommand(settings[0].QuestionnaireId, question,
                                                            settings[0].PropogationPublicKey));
            }
            catch (Exception e)
            {
                NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();
                logger.Fatal(e);
                return Json(new {question = questions[0], settings = settings[0], error = e.Message});
            }
            var model = viewRepository.Load<CompleteQuestionnaireViewInputModel, CompleteQuestionnaireJsonView>(
                new CompleteQuestionnaireViewInputModel(settings[0].QuestionnaireId, settings[0].ParentGroupPublicKey,
                                                        settings[0].PropogationPublicKey));
            return Json(model);
        }


        public JsonResult PropagateGroup(Guid publicKey, Guid parentGroupPublicKey, Guid questionnaireId)
        {
            try
            {
                var propagationKey = Guid.NewGuid();
                var commandService = NcqrsEnvironment.Get<ICommandService>();
                commandService.Execute(new AddPropagatableGroupCommand(questionnaireId, propagationKey,
                                                                       publicKey));
                var model = viewRepository.Load<CompleteQuestionnaireViewInputModel, CompleteQuestionnaireJsonView>(
                    new CompleteQuestionnaireViewInputModel(questionnaireId)
                        {CurrentGroupPublicKey = parentGroupPublicKey});
                return Json(new {propagationKey = propagationKey, parentGroupPublicKey = publicKey, group = model});
            }
            catch (Exception e)
            {
                ModelState.AddModelError("PropagationError", e.Message);
                return Json(new {error = e.Message, parentGroupPublicKey = publicKey});
            }
        }

        public JsonResult DeletePropagatedGroup(Guid propagationKey, Guid publicKey, Guid parentGroupPublicKey,
                                                string questionnaireId)
        {
            var commandService = NcqrsEnvironment.Get<ICommandService>();
            commandService.Execute(new DeletePropagatableGroupCommand(Guid.Parse(questionnaireId), propagationKey,
                                                                      publicKey));
            return Json(new {propagationKey = propagationKey});
        }

        public ActionResult Delete(string id)
        {
            var service = NcqrsEnvironment.Get<ICommandService>();
            service.Execute(new DeleteCompleteQuestionnaireCommand(Guid.Parse(id)));
            return RedirectToAction("Dashboard", "Survey");
        }

        #endregion

    }
}