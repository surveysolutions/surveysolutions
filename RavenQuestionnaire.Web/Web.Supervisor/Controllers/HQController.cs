using System.Web;
using Core.Supervisor.Views.User;

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

    using Main.Core.Commands.Questionnaire.Completed;
    using Main.Core.Entities.SubEntities;
    using Main.Core.View;
    using Main.Core.View.CompleteQuestionnaire;
    using Main.Core.View.CompleteQuestionnaire.Statistics;
    using Main.Core.View.Question;
    using Main.Core.View.Questionnaire;

    using Ncqrs.Commanding.ServiceModel;

    using Questionnaire.Core.Web.Helpers;

    using WB.Core.SharedKernel.Logger;

    using Web.Supervisor.Models;
    using Web.Supervisor.Models.Chart;

    using UserView = Main.Core.View.User.UserView;
    using UserViewInputModel = Main.Core.View.User.UserViewInputModel;

    [Authorize(Roles = "Headquarter")]
    public class HQController : BaseController
    {
        private readonly IViewFactory<CompleteQuestionnaireStatisticViewInputModel, CompleteQuestionnaireStatisticView> completeQuestionnaireStatisticViewFactory;
        private readonly IViewFactory<QuestionnaireBrowseInputModel, QuestionnaireBrowseView> questionnaireBrowseViewFactory;
        private readonly IViewFactory<UserListViewInputModel, UserListView> userListViewFactory;
        private readonly IViewFactory<IndexInputModel, IndexView> indexViewFactory;
        private readonly IViewFactory<StatusViewInputModel, StatusView> statusViewFactory;
        private readonly IViewFactory<AssignmentInputModel, AssignmentView> assignmentViewFactory;
        private readonly IViewFactory<AssignSurveyInputModel, AssignSurveyView> assignSurveyViewFactory;
        private readonly IViewFactory<InterviewersInputModel, InterviewersView> interviewersViewFactory;
        private readonly IViewFactory<DisplayViewInputModel, SurveyScreenView> surveyScreenViewFactory;
        private readonly IViewFactory<UserViewInputModel, UserView> userViewFactory;
        private readonly IViewFactory<SummaryInputModel, SummaryView> summaryViewFactory;
        private readonly IViewFactory<InterviewerStatisticsInputModel, InterviewerStatisticsView> interviewerStatisticsViewFactory;
        private readonly IViewFactory<InterviewerInputModel, InterviewerView> interviewerViewFactory;

        public HQController(
            IViewRepository viewRepository, ICommandService commandService, IGlobalInfoProvider provider, ILog logger,
            IViewFactory<CompleteQuestionnaireStatisticViewInputModel, CompleteQuestionnaireStatisticView> completeQuestionnaireStatisticViewFactory,
            IViewFactory<QuestionnaireBrowseInputModel, QuestionnaireBrowseView> questionnaireBrowseViewFactory,
            IViewFactory<UserListViewInputModel, UserListView> userListViewFactory,
            IViewFactory<IndexInputModel, IndexView> indexViewFactory,
            IViewFactory<StatusViewInputModel, StatusView> statusViewFactory,
            IViewFactory<AssignmentInputModel, AssignmentView> assignmentViewFactory,
            IViewFactory<AssignSurveyInputModel, AssignSurveyView> assignSurveyViewFactory,
            IViewFactory<InterviewersInputModel, InterviewersView> interviewersViewFactory,
            IViewFactory<DisplayViewInputModel, SurveyScreenView> surveyScreenViewFactory,
            IViewFactory<UserViewInputModel, UserView> userViewFactory,
            IViewFactory<SummaryInputModel, SummaryView> summaryViewFactory,
            IViewFactory<InterviewerStatisticsInputModel, InterviewerStatisticsView> interviewerStatisticsViewFactory,
            IViewFactory<InterviewerInputModel, InterviewerView> interviewerViewFactory)
            : base(viewRepository, commandService, provider, logger)
        {
            this.completeQuestionnaireStatisticViewFactory = completeQuestionnaireStatisticViewFactory;
            this.questionnaireBrowseViewFactory = questionnaireBrowseViewFactory;
            this.userListViewFactory = userListViewFactory;
            this.indexViewFactory = indexViewFactory;
            this.statusViewFactory = statusViewFactory;
            this.assignmentViewFactory = assignmentViewFactory;
            this.assignSurveyViewFactory = assignSurveyViewFactory;
            this.interviewersViewFactory = interviewersViewFactory;
            this.surveyScreenViewFactory = surveyScreenViewFactory;
            this.userViewFactory = userViewFactory;
            this.summaryViewFactory = summaryViewFactory;
            this.interviewerStatisticsViewFactory = interviewerStatisticsViewFactory;
            this.interviewerViewFactory = interviewerViewFactory;
        }

        public ActionResult Index()
        {
            var model = new HQDashboardModel
                {
                    Questionnaires =
                        this.questionnaireBrowseViewFactory.Load(
                            new QuestionnaireBrowseInputModel()),
                    Teams = this.userListViewFactory.Load(new UserListViewInputModel { Role = UserRoles.Supervisor })
                };
            return this.View(model);
        
        }

        public ActionResult TakeNew(string id)
        {
            Guid key;
            if (!Guid.TryParse(id, out key))
                throw new HttpException("404");
            var newQuestionnairePublicKey = Guid.NewGuid();
            this.CommandService.Execute(new CreateCompleteQuestionnaireCommand(newQuestionnairePublicKey, key, this.GlobalInfo.GetCurrentUser()));
            return this.RedirectToAction("Assign", "HQ", new { Id = newQuestionnairePublicKey});
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
                Logger.Fatal(e);
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

        public ActionResult Surveys(Guid? interviewerId)
        {
            ViewBag.ActivePage = MenuItem.Surveys;
            var model =
                this.indexViewFactory.Load(new IndexInputModel()
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
            var model = this.statusViewFactory.Load(new StatusViewInputModel()
                {
                    ViewerId = user.Id,
                    StatusId = statusId
                });
            ViewBag.GraphData = new StatusChartModel(model);
            return this.View(model);
        }

        public ActionResult Templates()
        {
            var model = this.questionnaireBrowseViewFactory.Load(new QuestionnaireBrowseInputModel()
                {
                    PageSize = int.MaxValue
                });
            return this.Json(model.Items.ToDictionary(item => item.QuestionnaireId.ToString(), item => item.Title), JsonRequestBehavior.AllowGet);
        }


        public ActionResult Documents(Guid? templateId, Guid? interviewerId, Guid? status, bool? isNotAssigned)
        {
            ViewBag.ActivePage = MenuItem.Docs;
            var user = this.GlobalInfo.GetCurrentUser();
            var inputModel = new AssignmentInputModel(user.Id,
                                       templateId,
                                       interviewerId, null, null, null,
                                       status,
                                       isNotAssigned ?? false);
            var model = this.assignmentViewFactory.Load(inputModel);
            ViewBag.Users = new SelectList(model.AssignableUsers, "PublicKey", "UserName");
            return this.View(model);
        }

        public ActionResult Assign(Guid id)
        {
            var user = this.GlobalInfo.GetCurrentUser();
            var model = this.assignSurveyViewFactory.Load(new AssignSurveyInputModel(id, user.Id));
            return this.View(model);
        }

        [HttpPost]
        public JsonResult Assign(AssignSuveyData data)
        {
            try
            {
                foreach (var answer in data.Answers)
                {
                    var answers = answer.Answers ?? new Guid[0];
                    this.CommandService.Execute(new SetAnswerCommand(data.QuestionnaireId, answer.Id, answers.ToList(), answer.Answer, null));
                }

                this.CommandService.Execute(new ChangeAssignmentCommand(data.QuestionnaireId, data.Responsible));
            }
            catch (Exception e)
            {
                Logger.Fatal(e);
                return Json(new { status = "error", error = e.Message }, JsonRequestBehavior.AllowGet);
            }
            return Json(new { status = "ok" }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult ChangeState(Guid id, string template)
        {
            var stat = this.completeQuestionnaireStatisticViewFactory.Load(
         new CompleteQuestionnaireStatisticViewInputModel(id) { Scope = QuestionScope.Supervisor });
            return this.View(new ApproveRedoModel() { Id = id, Statistic = stat, TemplateId = template });
        }


        public ActionResult StatusHistory(Guid id)
        {
            var stat = this.completeQuestionnaireStatisticViewFactory.Load(
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

                var stat = this.completeQuestionnaireStatisticViewFactory.Load(
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

                var stat = this.completeQuestionnaireStatisticViewFactory.Load(
                        new CompleteQuestionnaireStatisticViewInputModel(model.Id) { Scope = QuestionScope.Supervisor });
                return this.View(new ApproveRedoModel() { Id = model.Id, Statistic = stat, TemplateId = model.TemplateId });
            }
        }



        public ActionResult AssignPerson(Guid id, Guid tmptId)
        {
            var user = this.GlobalInfo.GetCurrentUser();
            var users = this.interviewersViewFactory.Load(new InterviewersInputModel { ViewerId = user.Id });
            var model = this.assignSurveyViewFactory.Load(new AssignSurveyInputModel(id, user.Id));
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


        public ActionResult Approve(Guid id, string template)
        {
            var stat = this.completeQuestionnaireStatisticViewFactory.Load(
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

            var stat = this.completeQuestionnaireStatisticViewFactory.Load(
                    new CompleteQuestionnaireStatisticViewInputModel(model.Id) { Scope = QuestionScope.Supervisor });
            return this.View(new ApproveRedoModel() { Id = model.Id, Statistic = stat, TemplateId = model.TemplateId });
        }


        public ActionResult Details(Guid id, string template, Guid? group, Guid? question, Guid? propagationKey)
        {
            ViewBag.ActivePage = MenuItem.Docs;
            var model = this.surveyScreenViewFactory.Load(
                new DisplayViewInputModel(id) { CurrentGroupPublicKey = group, PropagationKey = propagationKey, User = this.GlobalInfo.GetCurrentUser() });
            ViewBag.CurrentQuestion = question.HasValue ? question.Value : new Guid();
            ViewBag.TemplateId = template;
            return this.View(model);
        }

        public PartialViewResult Screen(Guid id, Guid group, Guid? propagationKey)
        {
            //if (string.IsNullOrEmpty(id))
            //    throw new HttpException(404, "Invalid query string parameters");
            var model = this.surveyScreenViewFactory.Load(
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

            var user = this.userViewFactory.Load(new UserViewInputModel(value));
            stat = this.completeQuestionnaireStatisticViewFactory.Load(new CompleteQuestionnaireStatisticViewInputModel(cqId) { Scope = QuestionScope.Supervisor });
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

            return this.RedirectToAction("Documents", "HQ", new { id = tmptId });
        }


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
                Logger.Fatal(e);
                return Json(new { status = "error", questionPublicKey = question.PublicKey, settings = settings[0], error = e.Message },
                            JsonRequestBehavior.AllowGet);
            }
            return Json(new { status = "ok" }, JsonRequestBehavior.AllowGet);
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
            var model = this.indexViewFactory.Load(input);
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
            var model = this.statusViewFactory.Load(input);
            ViewBag.GraphData = new StatusChartModel(model);
            return this.PartialView("_StatusTable", model);
        }


        public ActionResult AssignmentViewTable(GridDataRequestModel data)
        {
            var user = this.GlobalInfo.GetCurrentUser();
            var users = this.interviewersViewFactory.Load(new InterviewersInputModel { ViewerId = user.Id });
            ViewBag.Users = new SelectList(users.Items, "QuestionnaireId", "Login");
            var input = new AssignmentInputModel(GlobalInfo.GetCurrentUser().Id,
                data.TemplateId,
                data.InterviwerId,
                data.Pager.Page,
                data.Pager.PageSize,
                data.SortOrder,
                data.StatusId,
                false);
            var model = this.assignmentViewFactory.Load(input);
            return this.PartialView("_TableGroup", model);
        }


        public ActionResult ShowComments(Guid id, string template)
        {
            var model = this.surveyScreenViewFactory.Load(
                new DisplayViewInputModel(id));
            ViewBag.TemplateId = template;
            return this.View("Comments", model);
        }


        public ActionResult Redo(Guid id, string template)
        {
            var stat = this.completeQuestionnaireStatisticViewFactory.Load(
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

            var stat = this.completeQuestionnaireStatisticViewFactory.Load(
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
            var model = this.summaryViewFactory.Load(new SummaryInputModel(user));
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
            var model = this.summaryViewFactory.Load(input);
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
            var model = this.interviewerStatisticsViewFactory.Load(inputModel);
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
            var model = this.interviewerViewFactory.Load(input);
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
            var model = this.interviewerStatisticsViewFactory.Load(input);
            return this.PartialView("_UserStatistics", model);
        }

        public ActionResult UsersJson()
        {
            var user = this.GlobalInfo.GetCurrentUser();
            var input = new InterviewersInputModel { PageSize = int.MaxValue, ViewerId = user.Id };
            var model = this.interviewersViewFactory.Load(input);
            return this.Json(model.Items.ToDictionary(item => item.QuestionnaireId.ToString(), item => item.Login), JsonRequestBehavior.AllowGet);
        }
    }
}