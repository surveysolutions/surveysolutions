// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SurveyController.cs" company="World bank">
//   2012
// </copyright>
// <summary>
//   Defines the SurveyController type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

using WB.Core.SharedKernel.Utils.Logging;

namespace Web.Supervisor.Controllers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Web.Mvc;

    using Core.Supervisor.Views.Assignment;
    using Core.Supervisor.Views.Index;
    using Core.Supervisor.Views.Interviewer;
    using Core.Supervisor.Views.Status;
    using Core.Supervisor.Views.Summary;
    using Core.Supervisor.Views.Survey;

    using Main.Core.Commands.Questionnaire.Completed;
    using Main.Core.Entities.SubEntities;
    using Main.Core.View;
    using Main.Core.View.CompleteQuestionnaire.Statistics;
    using Main.Core.View.Questionnaire;
    using Main.Core.View.User;

    using Ncqrs.Commanding.ServiceModel;

    using Questionnaire.Core.Web.Helpers;

    using WB.Core.SharedKernel.Logger;

    using Web.Supervisor.Models;
    using Web.Supervisor.Models.Chart;

    using CompleteQuestionnaireViewInputModel = Main.Core.View.CompleteQuestionnaire.CompleteQuestionnaireViewInputModel;

    [Authorize(Roles = "Supervisor")]
    public class SurveyController : BaseController
    {
        public SurveyController(
            IViewRepository viewRepository, ICommandService commandService, IGlobalInfoProvider provider, ILog logger)
            : base(viewRepository, commandService, provider, logger)
        {
        }

        [HttpPost]
        public JsonResult AnswerQuestion(Guid surveyKey, Guid questionKey, Guid questionPropagationKey, Guid[] answers, string answerValue)
        {
            try
            {
                this.CommandService.Execute(
                    new SetAnswerCommand(
                        surveyKey,
                        questionKey,
                        new List<Guid>(answers ?? (new Guid[0])),
                        answerValue,
                        questionPropagationKey));
            }
            catch (Exception e)
            {
                var logger = LogManager.GetLogger(this.GetType());
                logger.Fatal(e);
                return Json(new { status = "error", error = e.Message }, JsonRequestBehavior.AllowGet);
            }

            return Json(new { status = "ok" }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult AddComment(Guid surveyKey, Guid questionKey, Guid questionPropagationKey, string comment)
        {
            try
            {
                var user = this.GlobalInfo.GetCurrentUser();
                this.CommandService.Execute(
                    new SetCommentCommand(
                        surveyKey,
                        questionKey,
                        comment,
                        (questionPropagationKey == Guid.Empty) ? (Guid?)null : questionPropagationKey,
                        user));
            }
            catch (Exception e)
            {
                Logger.Fatal(e);
                return this.Json(new { status = "error", error = e.Message });
            }

            return this.Json(new { status = "ok" });
        }

        public JsonResult FlagQuestion(Guid surveyKey, Guid questionKey, Guid? questionPropagationKey, bool isFlaged)
        {
            try
            {
                this.CommandService.Execute(
                    new SetFlagCommand(
                        surveyKey,
                        questionKey,
                        (questionPropagationKey == Guid.Empty) ? (Guid?)null : questionPropagationKey,
                         isFlaged)
                       );
            }
            catch (Exception e)
            {
                Logger.Fatal(e);
                return this.Json(new { status = "error", error = e.Message });
            }

            return this.Json(new { status = "ok" });
        }
        
        public ActionResult Index(Guid? interviewerId)
        {
            ViewBag.ActivePage = MenuItem.Surveys;
            var model =
                this.Repository.Load<IndexInputModel, IndexView>(new IndexInputModel()
                    {
                        InterviewerId = interviewerId,
                        ViewerId = GlobalInfo.GetCurrentUser().Id
                    });
            ViewBag.GraphData = new InterviewerChartModel(model);
            return this.View(model);
        }

        public ActionResult GotoBrowser()
        {
            return this.RedirectToAction("Index");
        }

        public ActionResult Status(Guid? statusId)
        {
            ViewBag.ActivePage = MenuItem.Statuses;
            var user = this.GlobalInfo.GetCurrentUser();
            var model = this.Repository.Load<StatusViewInputModel, StatusView>(new StatusViewInputModel()
                {
                    ViewerId = user.Id,
                    StatusId = statusId
                });
            ViewBag.GraphData = new StatusChartModel(model);
            return this.View(model);
        }

        public ActionResult Templates()
        {
            var model = this.Repository.Load<QuestionnaireBrowseInputModel, QuestionnaireBrowseView>(new QuestionnaireBrowseInputModel()
                {
                    PageSize = int.MaxValue
                });
            return this.Json(model.Items.ToDictionary(item => item.QuestionnaireId.ToString(), item => item.Title), JsonRequestBehavior.AllowGet);
        }

        public ActionResult Documents(Guid? templateId, Guid? interviewerId , Guid? status, bool? isNotAssigned)
        {
            ViewBag.ActivePage = MenuItem.Docs;
            var user = this.GlobalInfo.GetCurrentUser();
            var inputModel = new AssignmentInputModel(user.Id,
                templateId,
                interviewerId, null, null, null,
                status,
                isNotAssigned ?? false);
            var model = this.Repository.Load<AssignmentInputModel, AssignmentView>(inputModel);
            ViewBag.Users = new SelectList(model.AssignableUsers, "PublicKey", "UserName");
            return this.View(model);
        }

        public ActionResult ChangeState(Guid id, string template)
        {

            var stat = this.Repository.Load<CompleteQuestionnaireStatisticViewInputModel, CompleteQuestionnaireStatisticView>(
         new CompleteQuestionnaireStatisticViewInputModel(id) { Scope = QuestionScope.Supervisor });
            return this.View(new ApproveRedoModel() { Id = id, Statistic = stat, TemplateId = template });
        }


        public ActionResult StatusHistory(Guid id)
        {
            var stat = this.Repository.Load<CompleteQuestionnaireStatisticViewInputModel, CompleteQuestionnaireStatisticView>(
         new CompleteQuestionnaireStatisticViewInputModel(id) { Scope = QuestionScope.Supervisor });
            return this.PartialView("_StatusHistory", stat.StatusHistory);
        }

        [HttpPost]
        public ActionResult ChangeState(ApproveRedoModel model, int state, string cancel)
        {
            if (state == 2)
            {
                if (cancel != null)
                    return this.RedirectToAction("Documents", new { id = model.TemplateId });
                if (ModelState.IsValid)
                {
                    var status = SurveyStatus.Redo;
                    status.ChangeComment = model.Comment;
                    this.CommandService.Execute(
                        new ChangeStatusCommand()
                            {
                                CompleteQuestionnaireId = model.Id,
                                Status = status,
                                Responsible = this.GlobalInfo.GetCurrentUser()
                            });
                    return this.RedirectToAction("Documents", new { id = model.TemplateId });
                }

                var stat = this.Repository.Load<CompleteQuestionnaireStatisticViewInputModel, CompleteQuestionnaireStatisticView>(
                        new CompleteQuestionnaireStatisticViewInputModel(model.Id) { Scope = QuestionScope.Supervisor });
                return this.View(new ApproveRedoModel() { Id = model.Id, Statistic = stat, TemplateId = model.TemplateId });
            }
            else
            {
                if (ModelState.IsValid)
                {
                    var status = SurveyStatus.Approve;
                    status.ChangeComment = model.Comment;
                    this.CommandService.Execute(new ChangeStatusCommand() { CompleteQuestionnaireId = model.Id, Status = status, Responsible = this.GlobalInfo.GetCurrentUser() });
                    return this.RedirectToAction("Documents", new { id = model.TemplateId });
                }

                var stat = this.Repository.Load<CompleteQuestionnaireStatisticViewInputModel, CompleteQuestionnaireStatisticView>(
                        new CompleteQuestionnaireStatisticViewInputModel(model.Id) { Scope = QuestionScope.Supervisor });
                return this.View(new ApproveRedoModel() { Id = model.Id, Statistic = stat, TemplateId = model.TemplateId });
            }
        }


        public ActionResult Approve(Guid id, string template)
        {
            var stat = this.Repository.Load<CompleteQuestionnaireStatisticViewInputModel, CompleteQuestionnaireStatisticView>(
                    new CompleteQuestionnaireStatisticViewInputModel(id) { Scope = QuestionScope.Supervisor });
            return this.View(new ApproveRedoModel() { Id = id, Statistic = stat, TemplateId = template });
        }

        [HttpPost]
        public ActionResult Approve(ApproveRedoModel model)
        {
            if (ModelState.IsValid)
            {
                var status = SurveyStatus.Approve;
                status.ChangeComment = model.Comment;
                this.CommandService.Execute(new ChangeStatusCommand() { CompleteQuestionnaireId = model.Id, Status = status, Responsible = this.GlobalInfo.GetCurrentUser() });
                return this.RedirectToAction("Documents", new { id = model.TemplateId });
            }

            var stat = this.Repository.Load<CompleteQuestionnaireStatisticViewInputModel, CompleteQuestionnaireStatisticView>(
                    new CompleteQuestionnaireStatisticViewInputModel(model.Id) { Scope = QuestionScope.Supervisor });
            return this.View(new ApproveRedoModel() { Id = model.Id, Statistic = stat, TemplateId = model.TemplateId });
        }

        public ActionResult Details(Guid id, string template, Guid? group, Guid? question, Guid? propagationKey)
        {
            ViewBag.ActivePage = MenuItem.Docs;
            var model = this.Repository.Load<DisplayViewInputModel, SurveyScreenView>(
                new DisplayViewInputModel(id) { CurrentGroupPublicKey = group, PropagationKey = propagationKey, User = this.GlobalInfo.GetCurrentUser() });
            ViewBag.CurrentQuestion = question.HasValue ? question.Value : new Guid();
            ViewBag.TemplateId = template;
            return this.View(model);
        }

        public PartialViewResult Screen(Guid id, Guid group, Guid? propagationKey)
        {
            var model = this.Repository.Load<DisplayViewInputModel, SurveyScreenView>(
                new DisplayViewInputModel(id, group, propagationKey, this.GlobalInfo.GetCurrentUser()));
            ViewBag.CurrentQuestion = new Guid();
            ViewBag.PagePrefix = "";
            return this.PartialView("_SurveyContent", model);
        }

        [HttpPost]
        public ActionResult AssignForm(Guid cqId, Guid tmptId, Guid value)
        {
            UserLight responsible = null;
            CompleteQuestionnaireStatisticView stat = null;

            var user = this.Repository.Load<UserViewInputModel, UserView>(new UserViewInputModel(value));
            stat = this.Repository.Load<CompleteQuestionnaireStatisticViewInputModel, CompleteQuestionnaireStatisticView>(new CompleteQuestionnaireStatisticViewInputModel(cqId) { Scope = QuestionScope.Supervisor });
            responsible = (user != null) ? new UserLight(user.PublicKey, user.UserName) : new UserLight();
            if (stat.Status.PublicId == SurveyStatus.Unassign.PublicId)
            {
                stat.Status = SurveyStatus.Initial;
                this.CommandService.Execute(
                    new ChangeStatusCommand()
                        {
                            CompleteQuestionnaireId = cqId,
                            Status = SurveyStatus.Initial,
                            Responsible = this.GlobalInfo.GetCurrentUser()
                        });
            }

            this.CommandService.Execute(new ChangeAssignmentCommand(cqId, responsible));

            if (Request.IsAjaxRequest())
            {
                return this.Json(
                        new
                            {
                                status = "ok",
                                userId = responsible.Id,
                                userName = responsible.Name,
                                cqId = cqId,
                                statusName = stat.Status.Name,
                                statusId = stat.Status.PublicId
                            },
                        JsonRequestBehavior.AllowGet);
            }

            return this.RedirectToAction("Documents", "Survey", new { id = tmptId });
        }

        public ActionResult TableData(GridDataRequestModel data)
        {
            var input = new IndexInputModel
                            {
                                Page = data.Pager.Page,
                                PageSize = data.Pager.PageSize,
                                Orders = data.SortOrder,
                                InterviewerId = data.InterviwerId,
                                ViewerId = GlobalInfo.GetCurrentUser().Id
                            };
            var model = this.Repository.Load<IndexInputModel, IndexView>(input);
            ViewBag.GraphData = new InterviewerChartModel(model);
            return this.PartialView("_Table", model);
        }

        public ActionResult StatusViewTable(GridDataRequestModel data)
        {
            var user = this.GlobalInfo.GetCurrentUser();
            var input = new StatusViewInputModel
            {
                Page = data.Pager.Page,
                PageSize = data.Pager.PageSize,
                Orders = data.SortOrder,
                StatusId = data.StatusId,
                ViewerId = user.Id
            };
            var model = this.Repository.Load<StatusViewInputModel, StatusView>(input);
            ViewBag.GraphData = new StatusChartModel(model);
            return this.PartialView("_StatusTable", model);
        }

        public ActionResult AssignmentViewTable(GridDataRequestModel data)
        {
            var user = this.GlobalInfo.GetCurrentUser();
            var users = this.Repository.Load<InterviewersInputModel, InterviewersView>(new InterviewersInputModel { ViewerId = user.Id });
            ViewBag.Users = new SelectList(users.Items, "QuestionnaireId", "Login");
            var input = new AssignmentInputModel(GlobalInfo.GetCurrentUser().Id,
                data.TemplateId,
                data.InterviwerId,
                data.Pager.Page,
                data.Pager.PageSize,
                data.SortOrder,
                data.StatusId,
                false);
            var model = this.Repository.Load<AssignmentInputModel, AssignmentView>(input);
            return this.PartialView("_TableGroup", model);
        }

        public ActionResult ShowComments(Guid id, string template)
        {
            var model = this.Repository.Load<CompleteQuestionnaireViewInputModel, SurveyScreenView>(new CompleteQuestionnaireViewInputModel(id));
            ViewBag.TemplateId = template;
            return this.View("Comments", model);
        }

        public ActionResult Redo(Guid id, string template)
        {
            var stat = this.Repository.Load<CompleteQuestionnaireStatisticViewInputModel, CompleteQuestionnaireStatisticView>(
                    new CompleteQuestionnaireStatisticViewInputModel(id) { Scope = QuestionScope.Supervisor });
            return this.View(new ApproveRedoModel() { Id = id, TemplateId = template, Statistic = stat, StatusId = SurveyStatus.Redo.PublicId });
        }

        [HttpPost]
        public ActionResult Redo(ApproveRedoModel model, string redo, string cancel)
        {
            if (cancel != null)
                return this.RedirectToAction("Documents", new { id = model.TemplateId });
            if (ModelState.IsValid)
            {
                var status = SurveyStatus.Redo;
                status.ChangeComment = model.Comment;
                this.CommandService.Execute(new ChangeStatusCommand() { CompleteQuestionnaireId = model.Id, Status = status, Responsible = this.GlobalInfo.GetCurrentUser() });
                return this.RedirectToAction("Documents", new { id = model.TemplateId });
            }

            var stat = this.Repository.Load<CompleteQuestionnaireStatisticViewInputModel, CompleteQuestionnaireStatisticView>(
                    new CompleteQuestionnaireStatisticViewInputModel(model.Id) { Scope = QuestionScope.Supervisor });
            return this.View(new ApproveRedoModel() { Id = model.Id, Statistic = stat, TemplateId = model.TemplateId });
        }

        public ActionResult Chart(AssignmentView view)
        {
            var data = new ChartDataModel("Chart");
            if (view.Items.Count > 0)
            {
                var statusesName = view.Items.Select(t => t.Status.Name).Distinct().ToList();
                foreach (var state in statusesName)
                {
                    int count = view.Items.Where(t => t.Status.Name == state).Count();
                    data.Data.Add(state, count);
                }
            }

            return this.PartialView(data);
        }

        [Authorize]
        public ActionResult Summary()
        {
            ViewBag.ActivePage = MenuItem.Interviewers;
            var user = this.GlobalInfo.GetCurrentUser();
            var model = this.Repository.Load<SummaryInputModel, SummaryView>(new SummaryInputModel(user));
            ViewBag.GraphData = new SurveyChartModel(model);
            return this.View(model);
        }

        [Authorize]
        public ActionResult _SummaryData(GridDataRequestModel data)
        {
            var user = this.GlobalInfo.GetCurrentUser();
            var input = new SummaryInputModel(user)
            {
                Page = data.Pager.Page,
                PageSize = data.Pager.PageSize,
                Orders = data.SortOrder,
                TemplateId = data.TemplateId
            };
            var model = this.Repository.Load<SummaryInputModel, SummaryView>(input);
            ViewBag.GraphData = new SurveyChartModel(model);
            return this.PartialView("_SummaryTable", model);
        }

        public ActionResult Statistics(Guid id, InterviewerStatisticsInputModel input)
        {
            var inputModel = input == null
                ? new InterviewerStatisticsInputModel() { InterviewerId = id }
                : new InterviewerStatisticsInputModel()
                {
                    Order = input.Order,
                    Orders = input.Orders,
                    PageSize = input.PageSize,
                    Page = input.Page,
                    InterviewerId = id
                };
            var model = this.Repository.Load<InterviewerStatisticsInputModel, InterviewerStatisticsView>(inputModel);
            return this.View(model);
        }

        [HttpPost]
        public ActionResult TableGroupByUser(GridDataRequestModel data)
        {
            var input = new InterviewerInputModel()
            {
                Page = data.Pager.Page,
                PageSize = data.Pager.PageSize,
                Orders = data.SortOrder,
                TemplateId = data.TemplateId,
                InterviwerId = data.InterviwerId
            };
            var model = this.Repository.Load<InterviewerInputModel, InterviewerView>(input);
            return this.PartialView("_TableGroupByUser", model.Items[0]);
        }

        [HttpPost]
        public ActionResult UserStatistics(GridDataRequestModel data)
        {
            var input = new InterviewerStatisticsInputModel()
            {
                Page = data.Pager.Page,
                PageSize = data.Pager.PageSize,
                Orders = data.SortOrder,
                InterviewerId = data.InterviwerId
            };
            var model = this.Repository.Load<InterviewerStatisticsInputModel, InterviewerStatisticsView>(input);
            return this.PartialView("_UserStatistics", model);
        }
        
        public ActionResult UsersJson()
        {
            var user = this.GlobalInfo.GetCurrentUser();
            var input = new InterviewersInputModel { PageSize = int.MaxValue, ViewerId = user.Id };
            var model = this.Repository.Load<InterviewersInputModel, InterviewersView>(input);
            return this.Json(model.Items.ToDictionary(item => item.QuestionnaireId.ToString(), item => item.Login), JsonRequestBehavior.AllowGet);
        }
    }
}