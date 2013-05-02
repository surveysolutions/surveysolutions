// --------------------------------------------------------------------------------------------------------------------
// <copyright file="HQController.cs" company="">
//   
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
namespace Web.Supervisor.Controllers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Web.Mvc;

    using Core.Supervisor.Views.Assign;
    using Core.Supervisor.Views.Assignment;
    using Core.Supervisor.Views.Index;
    using Core.Supervisor.Views.Interviewer;
    using Core.Supervisor.Views.Status;
    using Core.Supervisor.Views.Summary;
    using Core.Supervisor.Views.Survey;
    using Core.Supervisor.Views.Timeline;

    using Main.Core.Commands.Questionnaire.Completed;
    using Main.Core.Entities.SubEntities;
    using Main.Core.View;
    using Main.Core.View.CompleteQuestionnaire;
    using Main.Core.View.CompleteQuestionnaire.Statistics;
    using Main.Core.View.Question;
    using Main.Core.View.Questionnaire;

    using Ncqrs.Commanding.ServiceModel;

    using NLog;

    using Questionnaire.Core.Web.Helpers;

    using Web.Supervisor.Models;
    using Web.Supervisor.Models.Chart;

    using UserView = Main.Core.View.User.UserView;
    using UserViewInputModel = Main.Core.View.User.UserViewInputModel;

    /// <summary>
    ///     The hq controller.
    /// </summary>
    [Authorize(Roles = "Headquarter")]
    public class HQController: BaseController
    {
         #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="SurveyController"/> class.
        /// </summary>
        /// <param name="viewRepository">
        /// The view repository.
        /// </param>
        /// <param name="commandService">
        /// The command Service.
        /// </param>
        /// <param name="provider">
        /// The provider.
        /// </param>
        public HQController(
            IViewRepository viewRepository, ICommandService commandService, IGlobalInfoProvider provider)
            : base(viewRepository, commandService, provider)
        {
        }

        #endregion

        #region Actions

        /// <summary>
        /// Save questionnaire answer in database 
        /// </summary>
        /// <param name="settings">
        /// The settings.
        /// </param>
        /// <param name="questions">
        /// The questions.
        /// </param>
        /// <returns>
        /// Return Json result
        /// </returns>
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
                var logger = NLog.LogManager.GetCurrentClassLogger();
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
                Logger logger = NLog.LogManager.GetCurrentClassLogger();
                logger.Fatal(e);
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
                Logger logger = NLog.LogManager.GetCurrentClassLogger();
                logger.Fatal(e);
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
            return this.Json(model.Items.ToDictionary(item => item.Id.ToString(), item => item.Title), JsonRequestBehavior.AllowGet);
        }


        public ActionResult Documents(Guid? templateId, Guid? interviewerId , Guid? status, bool? isNotAssigned)
        {
            ViewBag.ActivePage = MenuItem.Docs;
            var inputModel = new AssignmentInputModel(GlobalInfo.GetCurrentUser().Id,
                                       templateId,
                                       interviewerId,null,null,null,
                                       status,
                                       isNotAssigned ?? false);
            var user = this.GlobalInfo.GetCurrentUser();
            var model = this.Repository.Load<AssignmentInputModel, AssignmentView>(inputModel);
            var users = this.Repository.Load<InterviewersInputModel, InterviewersView>(new InterviewersInputModel { ViewerId = user.Id });
            ViewBag.Users = new SelectList(users.Items, "QuestionnaireId", "Login");
            return this.View(model);
        }

        /// <summary>
        /// Display assign form
        /// </summary>
        /// <param name="id">
        /// The id.
        /// </param>
        /// <returns>
        /// Return Assign form
        /// </returns>
        public ActionResult Assign(Guid id)
        {
            var model = this.Repository.Load<AssignSurveyInputModel, AssignSurveyView>(new AssignSurveyInputModel(id));
            return this.View(model);
        }

        /// <summary>
        /// Display change state
        /// </summary>
        /// <param name="id">
        /// The id.
        /// </param>
        /// <param name="template">
        /// The template.
        /// </param>
        /// <returns>
        /// Return Approve Page
        /// </returns>
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
        /// <summary>
        /// Save change state in database
        /// </summary>
        /// <param name="model">
        /// The model.
        /// </param>
        /// <returns>
        /// Return view
        /// </returns>
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


        /// <summary>
        /// Display assign form
        /// </summary>
        /// <param name="id">
        /// The id.
        /// </param>
        /// <param name="tmptId">
        /// The tmpt QuestionnaireId.
        /// </param>
        /// <returns>
        /// Return Assign form
        /// </returns>
        public ActionResult AssignPerson(Guid id, Guid tmptId)
        {
            var user = this.GlobalInfo.GetCurrentUser();
            var users = this.Repository.Load<InterviewersInputModel, InterviewersView>(new InterviewersInputModel { ViewerId = user.Id });
            var model = this.Repository.Load<AssignSurveyInputModel, AssignSurveyView>(new AssignSurveyInputModel(id));
            var r = users.Items.ToList();
            var options = r.Select(item => new SelectListItem
            {
                Value = item.QuestionnaireId.ToString(),
                Text = item.Login,
                Selected = (model.Responsible != null && model.Responsible.Id == item.QuestionnaireId) || (model.Responsible == null && item.QuestionnaireId == Guid.Empty)
            }).ToList();
            ViewBag.value = options;
            return this.View(model);
        }

        /// <summary>
        /// Display approve page
        /// </summary>
        /// <param name="id">
        /// The id.
        /// </param>
        /// <param name="template">
        /// The template.
        /// </param>
        /// <returns>
        /// Return Approve Page
        /// </returns>
        public ActionResult Approve(Guid id, string template)
        {
            var stat = this.Repository.Load<CompleteQuestionnaireStatisticViewInputModel, CompleteQuestionnaireStatisticView>(
                    new CompleteQuestionnaireStatisticViewInputModel(id) { Scope = QuestionScope.Supervisor });
            return this.View(new ApproveRedoModel() { Id = id, Statistic = stat, TemplateId = template });
        }

        /// <summary>
        /// Save approve status in database
        /// </summary>
        /// <param name="model">
        /// The model.
        /// </param>
        /// <returns>
        /// Return view
        /// </returns>
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

        /// <summary>
        /// Display details page
        /// </summary>
        /// <param name="id">
        /// The id.
        /// </param>
        /// <param name="template">
        /// The template.
        /// </param>
        /// <param name="group">
        /// The group.
        /// </param>
        /// <param name="question">
        /// The question.
        /// </param>
        /// <param name="propagationKey">
        /// The propagation key.
        /// </param>
        /// <returns>
        /// Return details page
        /// </returns>
        public ActionResult Details(Guid id, string template, Guid? group, Guid? question, Guid? propagationKey)
        {
            ViewBag.ActivePage = MenuItem.Docs;
            var model = this.Repository.Load<DisplayViewInputModel, SurveyScreenView>(
                new DisplayViewInputModel(id) { CurrentGroupPublicKey = group, PropagationKey = propagationKey, User = this.GlobalInfo.GetCurrentUser() });
            ViewBag.CurrentQuestion = question.HasValue ? question.Value : new Guid();
            ViewBag.TemplateId = template;
            return this.View(model);
        }

        public ActionResult Timeline(Guid id)
        {
            var model = this.Repository.Load<TimelineViewInputModel, TimelineView>(new TimelineViewInputModel(id));
            return this.View(model);
        }

        /// <summary>
        /// Display part of questionnaire content
        /// </summary>
        /// <param name="id">
        /// The id.
        /// </param>
        /// <param name="group">
        /// The group.
        /// </param>
        /// <param name="propagationKey">
        /// The propagation key.
        /// </param>
        /// <returns>
        /// Return partial view with questionnaire content
        /// </returns>
        public PartialViewResult Screen(Guid id, Guid group, Guid? propagationKey)
        {
            //if (string.IsNullOrEmpty(id))
            //    throw new HttpException(404, "Invalid query string parameters");
            var model = this.Repository.Load<DisplayViewInputModel, SurveyScreenView>(
                new DisplayViewInputModel(id, group, propagationKey, this.GlobalInfo.GetCurrentUser()));
            ViewBag.CurrentQuestion = new Guid();
            ViewBag.PagePrefix = "";
            return this.PartialView("_SurveyContent", model);
        }

        /// <summary>
        /// Save responsible for questionnaire on database
        /// </summary>
        /// <param name="cqId">
        /// The cq id.
        /// </param>
        /// <param name="tmptId">
        /// The tmpt id.
        /// </param>
        /// <param name="value">
        /// The user id.
        /// </param>
        /// <returns>
        /// Return redirect on assigments page
        /// </returns>
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

        /// <summary>
        /// Save questionnaire answer in database 
        /// </summary>
        /// <param name="settings">
        /// The settings.
        /// </param>
        /// <param name="questions">
        /// The questions.
        /// </param>
        /// <returns>
        /// Return Json result
        /// </returns>
        [HttpPost]
        public JsonResult SaveAnswer(CompleteQuestionSettings[] settings, CompleteQuestionView[] questions)
        {
            var question = questions[0];

            List<Guid> answers = new List<Guid>();
            string completeAnswerValue = null;

            try
            {
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

                this.CommandService.Execute(
                    new SetAnswerCommand(
                        settings[0].QuestionnaireId,
                        question.PublicKey,
                        answers,
                        completeAnswerValue,
                        settings[0].PropogationPublicKey));
            }
            catch (Exception e)
            {
                var logger = NLog.LogManager.GetCurrentClassLogger();
                logger.Fatal(e);
                return Json(new { status = "error", questionPublicKey = question.PublicKey, settings = settings[0], error = e.Message },
                            JsonRequestBehavior.AllowGet);
            }
            return Json(new { status = "ok" }, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// Display sorting questionnaire
        /// </summary>
        /// <param name="data">
        /// The data.
        /// </param>
        /// <returns>
        /// Return sorted partial view
        /// </returns>
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

        /// <summary>
        /// Display sorting questionnaire
        /// </summary>
        /// <param name="data">
        /// The data.
        /// </param>
        /// <returns>
        /// Return sorted partial view
        /// </returns>
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

        /// <summary>
        /// Display sorting questionnaire on Survey page
        /// </summary>
        /// <param name="data">
        /// The data.
        /// </param>
        /// <returns>
        /// Return sorted partial view
        /// </returns>
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

        /// <summary>
        /// Upload and display comments
        /// </summary>
        /// <param name="id">
        /// The id.
        /// </param>
        /// /// <param name="template">
        /// The template.
        /// </param>
        /// <returns>
        /// Return page with comments
        /// </returns>
        public ActionResult ShowComments(Guid id, string template)
        {
            var model = this.Repository.Load<CompleteQuestionnaireViewInputModel, SurveyScreenView>(
                new CompleteQuestionnaireViewInputModel(id));
            ViewBag.TemplateId = template;
            return this.View("Comments", model);
        }

        /// <summary>
        /// ability to sign status Redo for questionnaire
        /// </summary>
        /// <param name="id">
        /// The id.
        /// </param>
        /// <param name="template">
        /// The template.
        /// </param>
        /// <returns>
        /// Return page with ability to came back questionnaire
        /// </returns>
        public ActionResult Redo(Guid id, string template)
        {
            var stat = this.Repository.Load<CompleteQuestionnaireStatisticViewInputModel, CompleteQuestionnaireStatisticView>(
                    new CompleteQuestionnaireStatisticViewInputModel(id) { Scope = QuestionScope.Supervisor });
            return this.View(new ApproveRedoModel() { Id = id, TemplateId = template, Statistic = stat, StatusId = SurveyStatus.Redo.PublicId });
        }

        /// <summary>
        /// Save redo status in database
        /// </summary>
        /// <param name="model">
        /// The model.
        /// </param>
        /// <param name="redo">
        /// The redo.
        /// </param>
        /// <param name="cancel">
        /// The cancel.
        /// </param>
        /// <returns>
        /// return view
        /// </returns>
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

        /// <summary>
        /// Action for preparing data for visual chart
        /// </summary>
        /// <param name="view">
        /// The view.
        /// </param>
        /// <returns>
        /// return Partial View with visual chart
        /// </returns>
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

        /// <summary>
        /// Interviewer summary view
        /// </summary>
        /// <returns>
        /// Interviewer summary view
        /// </returns>
        [Authorize]
        public ActionResult Summary()
        {
            ViewBag.ActivePage = MenuItem.Interviewers;
            var user = this.GlobalInfo.GetCurrentUser();
            var model = this.Repository.Load<SummaryInputModel, SummaryView>(new SummaryInputModel(user));
            ViewBag.GraphData = new SurveyChartModel(model);
            return this.View(model);
        }

        /// <summary>
        /// Gets table data for some view
        /// </summary>
        /// <param name="data">
        /// The data.
        /// </param>
        /// <returns>
        /// Partial view with table's body
        /// </returns>
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

        /// <summary>
        /// Display user's statistics grouped by surveys and statuses
        /// </summary>
        /// <param name="id">
        /// The id.
        /// </param>
        /// <param name="input">
        /// The input.
        /// </param>
        /// <returns>
        /// Show statistics view if everything is ok
        /// </returns>
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

        /// <summary>
        /// Gets table data for some view
        /// </summary>
        /// <param name="data">
        /// The data.
        /// </param>
        /// <returns>
        /// Partial view with table's body
        /// </returns>
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

        /// <summary>
        /// Gets user's statistics
        /// </summary>
        /// <param name="data">
        /// Table order data
        /// </param>
        /// <returns>
        /// Partial view with table's body
        /// </returns>
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

        /// <summary>
        /// Uses to filter grids by user
        /// </summary>
        /// <returns>
        /// List of all  supervisor's users
        /// </returns>
        public ActionResult UsersJson()
        {
            var user = this.GlobalInfo.GetCurrentUser();
            var input = new InterviewersInputModel { PageSize = int.MaxValue, ViewerId = user.Id };
            var model = this.Repository.Load<InterviewersInputModel, InterviewersView>(input);
            return this.Json(model.Items.ToDictionary(item => item.QuestionnaireId.ToString(), item => item.Login), JsonRequestBehavior.AllowGet);
        }

        #endregion
    }
}