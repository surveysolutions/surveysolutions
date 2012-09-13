using Ncqrs;
using System;
using System.Web.Mvc;
using Web.Supervisor.Models;
using RavenQuestionnaire.Core;
using Ncqrs.Commanding.ServiceModel;
using Questionnaire.Core.Web.Helpers;
using RavenQuestionnaire.Core.Views.User;
using RavenQuestionnaire.Core.Views.Assign;
using RavenQuestionnaire.Core.Views.Survey;
using RavenQuestionnaire.Core.Views.Question;
using RavenQuestionnaire.Core.Views.Statistics;
using Main.Core.Entities.SubEntities;
using RavenQuestionnaire.Core.Views.CompleteQuestionnaire;
using Main.Core.Commands.Questionnaire.Completed;
using RavenQuestionnaire.Core.Views.CompleteQuestionnaire.Mobile;


namespace Web.Supervisor.Controllers
{
    using System.Collections.Generic;
    using System.Linq;

    using Main.Core.Commands.Questionnaire.Completed;
    using Main.Core.Entities.SubEntities;

    using RavenQuestionnaire.Core.Views.Interviewer;

    [Authorize]
    public class SurveyController : Controller
    {
        private readonly IGlobalInfoProvider globalInfo;
        private readonly IViewRepository viewRepository;


        public SurveyController(IViewRepository viewRepository,

                                IGlobalInfoProvider provider)
        {
            this.viewRepository = viewRepository;
            globalInfo = provider;
        }

        public ActionResult Index(SurveyViewInputModel input)
        {
            SurveyBrowseView model = viewRepository.Load<SurveyViewInputModel, SurveyBrowseView>(input);
            return View(model);
        }

        public ActionResult Assigments(Guid id, SurveyGroupInputModel input, string status)
        {
            var inputModel = input == null?new SurveyGroupInputModel() { Id = id, Status = status} : new SurveyGroupInputModel(id, input.Page, input.PageSize, input.Orders, status);
            var user = globalInfo.GetCurrentUser();
            SurveyGroupView model = viewRepository.Load<SurveyGroupInputModel, SurveyGroupView>(inputModel);
            var users = viewRepository.Load<InterviewersInputModel, InterviewersView>(new InterviewersInputModel { Supervisor = user });
            ViewBag.Users = new SelectList(users.Items, "Id", "Login");
            return View(model);
        }

        public ActionResult Assign(Guid id)
        {
            UserLight user = globalInfo.GetCurrentUser();
            InterviewersView users = viewRepository.Load<InterviewersInputModel, InterviewersView>(new InterviewersInputModel { Supervisor = user });
            AssignSurveyView model = viewRepository.Load<AssignSurveyInputModel, AssignSurveyView>(new AssignSurveyInputModel(id));
            var r = users.Items.ToList();
            r.Insert(0, new InterviewersItem(Guid.Empty, string.Empty, string.Empty, DateTime.MinValue, false, 0, 0, 0));
            var options = r.Select(item => new SelectListItem
                {
                    Value = item.Id.ToString(),
                    Text = item.Login,
                    Selected = (model.Responsible != null && model.Responsible.Id == item.Id) || (model.Responsible == null && item.Id == Guid.Empty)
                }).ToList();
            ViewBag.userId = options;
            return View(model);
        }

        public ActionResult Approve(Guid id, string template)
        {
            var stat = viewRepository.Load<CompleteQuestionnaireStatisticViewInputModel, CompleteQuestionnaireStatisticView>(
                    new CompleteQuestionnaireStatisticViewInputModel(id.ToString()));
            
            return View(new ApproveModel() { Id = id, Statistic = stat, TemplateId = template });
        }

        [HttpPost]
        public ActionResult Approve(ApproveModel model)
        {
            if (ModelState.IsValid)
            {
                var commandService = NcqrsEnvironment.Get<ICommandService>();
                var status = SurveyStatus.Approve;
                status.ChangeComment = model.Comment;
                commandService.Execute(new ChangeStatusCommand() { CompleteQuestionnaireId = model.Id, Status = status });
                return RedirectToAction("Assigments", new { id = model.TemplateId });
            }
            else
            {
                var stat = viewRepository.Load
                    <CompleteQuestionnaireStatisticViewInputModel, CompleteQuestionnaireStatisticView>(
                        new CompleteQuestionnaireStatisticViewInputModel(model.Id.ToString()));
                return View(new ApproveModel() { Id = model.Id, Statistic = stat, TemplateId = model.TemplateId });
            }
        }

        public ActionResult Details(Guid id, string template, Guid? group, Guid? question, Guid? propagationKey)
        {
            //if (id)
            //    throw new HttpException(404, "Invalid query string parameters");
            var model = viewRepository.Load<CompleteQuestionnaireViewInputModel, CompleteQuestionnaireMobileView>(
                new CompleteQuestionnaireViewInputModel(id) { CurrentGroupPublicKey = group, PropagationKey = propagationKey });
            ViewBag.CurrentQuestion = question.HasValue ? question.Value : new Guid();
            ViewBag.TemplateId = template;
            return View(model);
        }

        public PartialViewResult Screen(Guid id, Guid group, Guid? propagationKey)
        {
            //if (string.IsNullOrEmpty(id))
            //    throw new HttpException(404, "Invalid query string parameters");
            var model = viewRepository.Load<CompleteQuestionnaireViewInputModel, CompleteGroupMobileView>(
                new CompleteQuestionnaireViewInputModel(id, group, propagationKey));
            ViewBag.CurrentQuestion = new Guid();
            ViewBag.PagePrefix = "";
            return PartialView("_SurveyContent", model);
        }

        [HttpPost]
        public ActionResult AssignForm(Guid CqId, Guid TmptId, Guid userId)
        {
            UserLight responsible = null;
            try
            {
                UserView user = viewRepository.Load<UserViewInputModel, UserView>(new UserViewInputModel(userId));
                responsible = (user != null) ? new UserLight(user.PublicKey, user.UserName) : new UserLight();
                var commandService = NcqrsEnvironment.Get<ICommandService>();
                commandService.Execute(new ChangeAssignmentCommand(CqId, responsible));
            }
            catch (Exception e)
            {
                return Json(new { status = "error", error = e.Message }, JsonRequestBehavior.AllowGet);
            }
            if (Request.IsAjaxRequest())
                return Json(new { status = "ok", userId = responsible.Id, userName = responsible.Name, cqId = CqId },JsonRequestBehavior.AllowGet);
            return RedirectToAction("Assigments", "Survey", new { id = TmptId });
        }

        [HttpPost]
        public JsonResult SaveAnswer(CompleteQuestionSettings[] settings, CompleteQuestionView[] questions)
        {
            var question = questions[0];

            List<Guid> answers = new List<Guid>();
            string completeAnswerValue = null;

            try
            {
                var commandService = NcqrsEnvironment.Get<ICommandService>();

                if (question.QuestionType == QuestionType.DropDownList ||
                question.QuestionType == QuestionType.SingleOption ||
                question.QuestionType == QuestionType.YesNo ||
                question.QuestionType == QuestionType.MultyOption)
                {
                    if (question.Answers != null && question.Answers.Length > 0)
                    {
                        for (int i = 0; i < question.Answers.Length; i++)
                        {
                            if (question.Answers[i].Selected)
                            {
                                answers.Add(question.Answers[i].PublicKey);
                            }
                        }
                    }
                }
                else
                {
                    completeAnswerValue = question.Answers[0].AnswerValue;
                }

                commandService.Execute(
                    new SetAnswerCommand(
                        settings[0].QuestionnaireId,
                        question.PublicKey,
                        answers,
                        completeAnswerValue,
                        settings[0].PropogationPublicKey));
            }
            catch (Exception e)
            {
                NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();
                logger.Fatal(e);
                return Json(new { status = "error", questionPublicKey = question.PublicKey, settings = settings[0], error = e.Message },
                            JsonRequestBehavior.AllowGet);
            }
            return Json(new { status = "ok" }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult _TableData(GridDataRequestModel data)
        {
            var input = new SurveyViewInputModel
                            {
                                Page = data.Pager.Page,
                                PageSize = data.Pager.PageSize,
                                Orders = data.SortOrder
                            };
            var model = viewRepository.Load<SurveyViewInputModel, SurveyBrowseView>(input);
            return PartialView("_Table", model);
        }

        public ActionResult _GroupTableData(GridDataRequestModel data)
        {
            UserLight user = globalInfo.GetCurrentUser();
            var users =
                viewRepository.Load<InterviewersInputModel, InterviewersView>(new InterviewersInputModel { Supervisor = user });
            ViewBag.Users = new SelectList(users.Items, "Id", "Login");
            var input = new SurveyGroupInputModel(data.Id, data.Pager.Page, data.Pager.PageSize, data.SortOrder);
            var model = viewRepository.Load<SurveyGroupInputModel, SurveyGroupView>(input);
            return PartialView("_TableGroup", model);
        }
    }
}