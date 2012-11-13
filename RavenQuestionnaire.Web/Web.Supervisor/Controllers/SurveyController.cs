// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SurveyController.cs" company="World bank">
//   2012
// </copyright>
// <summary>
//   Defines the SurveyController type.
// </summary>
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

    using Main.Core.Commands.Questionnaire.Completed;
    using Main.Core.Commands.Synchronization;
    using Main.Core.Entities;
    using Main.Core.Entities.SubEntities;
    using Main.Core.Events.Synchronization;
    using Main.Core.Services;
    using Main.Core.View;
    using Main.Core.View.CompleteQuestionnaire;
    using Main.Core.View.CompleteQuestionnaire.ScreenGroup;
    using Main.Core.View.CompleteQuestionnaire.Statistics;
    using Main.Core.View.Group;
    using Main.Core.View.Question;
    using Main.Core.View.Questionnaire;
    using Main.Core.View.User;

    using Ncqrs;
    using Ncqrs.Commanding.ServiceModel;

    using Questionnaire.Core.Web.Helpers;

    using Web.Supervisor.Models;

    /// <summary>
    /// Responsible for display surveys and statistic info about surveys
    /// </summary>
    [Authorize]
    public class SurveyController : Controller
    {
        #region Fields

        /// <summary>
        /// Global info object
        /// </summary>
        private readonly IGlobalInfoProvider globalInfo;

        /// <summary>
        /// View Repository object
        /// </summary>
        private readonly IViewRepository viewRepository;

        #endregion

        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="SurveyController"/> class.
        /// </summary>
        /// <param name="viewRepository">
        /// The view repository.
        /// </param>
        /// <param name="provider">
        /// The provider.
        /// </param>
        public SurveyController(IViewRepository viewRepository, IGlobalInfoProvider provider)
        {
            this.viewRepository = viewRepository;
            this.globalInfo = provider;
        }

        #endregion

        #region Actions

        /// <summary>
        /// Display statistic surveys on page
        /// </summary>
        /// <param name="input">
        /// The input.
        /// </param>
        /// <returns>
        /// Return index page
        /// </returns>
        public ActionResult Index(IndexInputModel input)
        {
            var model = this.viewRepository.Load<IndexInputModel, IndexView>(input);
            return this.View(model);
        }

        public ActionResult Templates()
        {
            var model = this.viewRepository.Load<QuestionnaireBrowseInputModel, QuestionnaireBrowseView>(new QuestionnaireBrowseInputModel()
                {
                    PageSize = int.MaxValue
                });
            return this.Json(model.Items.ToDictionary(item => item.Id.ToString(), item => item.Title), JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// Display Assigments statistic
        /// </summary>
        /// <param name="id">
        /// The id.
        /// </param>
        /// <param name="userId">
        /// The user Id.
        /// </param>
        /// <param name="input">
        /// The input.
        /// </param>
        /// <param name="status">
        /// The status.
        /// </param>
        /// <param name="isNotAssigned">
        /// The isNotAssigned.
        /// </param>
        /// <returns>
        /// Return Assigments page
        /// </returns>
        public ActionResult Assigments(Guid id, Guid? userId, AssignmentInputModel input, ICollection<string> status, bool? isNotAssigned)
        {
            var inputModel = input == null
                                 ? new AssignmentInputModel()
                                     {
                                         Id = id,
                                         Statuses = status,
                                         UserId = userId.HasValue ? userId.Value : Guid.Empty
                                     }
                                 : new AssignmentInputModel(
                                       id,
                                       userId.HasValue ? userId.Value : Guid.Empty,
                                       input.Page,
                                       input.PageSize,
                                       input.Orders,
                                       status,
                                       isNotAssigned ?? false);
            var user = this.globalInfo.GetCurrentUser();
            var model = this.viewRepository.Load<AssignmentInputModel, AssignmentView>(inputModel);
            var users = this.viewRepository.Load<InterviewersInputModel, InterviewersView>(new InterviewersInputModel { Supervisor = user });
            ViewBag.Users = new SelectList(users.Items, "Id", "Login");
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
            var model = this.viewRepository.Load<AssignSurveyInputModel, AssignSurveyView>(new AssignSurveyInputModel(id));
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

            var stat = this.viewRepository.Load<CompleteQuestionnaireStatisticViewInputModel, CompleteQuestionnaireStatisticView>(
         new CompleteQuestionnaireStatisticViewInputModel(id));
            return this.View(new ApproveRedoModel() { Id = id, Statistic = stat, TemplateId = template });
        }


        public ActionResult StatusHistory(Guid id)
        {
            var stat = this.viewRepository.Load<CompleteQuestionnaireStatisticViewInputModel, CompleteQuestionnaireStatisticView>(
         new CompleteQuestionnaireStatisticViewInputModel(id));
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
                    return this.RedirectToAction("Assigments", new { id = model.TemplateId });
                if (ModelState.IsValid)
                {
                    var commandService = NcqrsEnvironment.Get<ICommandService>();
                    var status = SurveyStatus.Redo;
                    status.ChangeComment = model.Comment;
                    commandService.Execute(
                        new ChangeStatusCommand()
                            {
                                CompleteQuestionnaireId = model.Id,
                                Status = status,
                                Responsible = this.globalInfo.GetCurrentUser()
                            });
                    return this.RedirectToAction("Assigments", new { id = model.TemplateId });
                }

                var stat = this.viewRepository.Load<CompleteQuestionnaireStatisticViewInputModel, CompleteQuestionnaireStatisticView>(
                        new CompleteQuestionnaireStatisticViewInputModel(model.Id));
                return this.View(new ApproveRedoModel() { Id = model.Id, Statistic = stat, TemplateId = model.TemplateId });
            }
            else
            {
                if (ModelState.IsValid)
                {
                    var commandService = NcqrsEnvironment.Get<ICommandService>();
                    var status = SurveyStatus.Approve;
                    status.ChangeComment = model.Comment;
                    commandService.Execute(new ChangeStatusCommand() { CompleteQuestionnaireId = model.Id, Status = status, Responsible = this.globalInfo.GetCurrentUser() });
                    return this.RedirectToAction("Assigments", new { id = model.TemplateId });
                }

                var stat = this.viewRepository.Load<CompleteQuestionnaireStatisticViewInputModel, CompleteQuestionnaireStatisticView>(
                        new CompleteQuestionnaireStatisticViewInputModel(model.Id));
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
        /// The tmpt Id.
        /// </param>
        /// <returns>
        /// Return Assign form
        /// </returns>
        public ActionResult AssignPerson(Guid id, Guid tmptId)
        {
            var user = this.globalInfo.GetCurrentUser();
            var users = this.viewRepository.Load<InterviewersInputModel, InterviewersView>(new InterviewersInputModel { Supervisor = user });
            var model = this.viewRepository.Load<AssignSurveyInputModel, AssignSurveyView>(new AssignSurveyInputModel(id));
            var r = users.Items.ToList();
            r.Insert(0, new InterviewersItem(Guid.Empty, string.Empty, string.Empty, DateTime.MinValue, false));
            var options = r.Select(item => new SelectListItem
            {
                Value = item.Id.ToString(),
                Text = item.Login,
                Selected = (model.Responsible != null && model.Responsible.Id == item.Id) || (model.Responsible == null && item.Id == Guid.Empty)
            }).ToList();
            ViewBag.userId = options;
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
            var stat = this.viewRepository.Load<CompleteQuestionnaireStatisticViewInputModel, CompleteQuestionnaireStatisticView>(
                    new CompleteQuestionnaireStatisticViewInputModel(id));
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
                var commandService = NcqrsEnvironment.Get<ICommandService>();
                var status = SurveyStatus.Approve;
                status.ChangeComment = model.Comment;
                commandService.Execute(new ChangeStatusCommand() { CompleteQuestionnaireId = model.Id, Status = status, Responsible = this.globalInfo.GetCurrentUser() });
                return this.RedirectToAction("Assigments", new { id = model.TemplateId });
            }

            var stat = this.viewRepository.Load<CompleteQuestionnaireStatisticViewInputModel, CompleteQuestionnaireStatisticView>(
                    new CompleteQuestionnaireStatisticViewInputModel(model.Id));
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
            //if (id)
            //    throw new HttpException(404, "Invalid query string parameters");
            var model = this.viewRepository.Load<DisplaViewInputModel, ScreenGroupView>(
                new DisplaViewInputModel(id) { CurrentGroupPublicKey = group, PropagationKey = propagationKey });
            ViewBag.CurrentQuestion = question.HasValue ? question.Value : new Guid();
            ViewBag.TemplateId = template;
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
            var model = this.viewRepository.Load<DisplaViewInputModel, ScreenGroupView>(
                new DisplaViewInputModel(id, group, propagationKey));
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
        /// <param name="userId">
        /// The user id.
        /// </param>
        /// <returns>
        /// Return redirect on assigments page
        /// </returns>
        [HttpPost]
        public ActionResult AssignForm(Guid cqId, Guid tmptId, Guid userId)
        {
            UserLight responsible = null;
            try
            {
                var user = this.viewRepository.Load<UserViewInputModel, UserView>(new UserViewInputModel(userId));
                responsible = (user != null) ? new UserLight(user.PublicKey, user.UserName) : new UserLight();
                var commandService = NcqrsEnvironment.Get<ICommandService>();
                commandService.Execute(new ChangeAssignmentCommand(cqId, responsible));
            }
            catch (Exception e)
            {
                return Json(new { status = "error", error = e.Message }, JsonRequestBehavior.AllowGet);
            }

            if (Request.IsAjaxRequest())
            {
                return Json(
                    new { status = "ok", userId = responsible.Id, userName = responsible.Name, cqId = cqId },
                    JsonRequestBehavior.AllowGet);
            }
            return this.RedirectToAction("Assigments", "Survey", new { id = tmptId });
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
                                UserId = data.UserId
                            };
            var model = this.viewRepository.Load<IndexInputModel, IndexView>(input);
            return this.PartialView("_Table", model);
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
        public ActionResult GroupTableData(GridDataRequestModel data)
        {
            var user = this.globalInfo.GetCurrentUser();
            var users = this.viewRepository.Load<InterviewersInputModel, InterviewersView>(new InterviewersInputModel { Supervisor = user });
            ViewBag.Users = new SelectList(users.Items, "Id", "Login");
            var input = new AssignmentInputModel(data.Id, data.UserId, data.Pager.Page, data.Pager.PageSize, data.SortOrder);
            var model = this.viewRepository.Load<AssignmentInputModel, AssignmentView>(input);
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
            var model = this.viewRepository.Load<CompleteQuestionnaireViewInputModel, ScreenGroupView>(
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
            var stat = this.viewRepository.Load<CompleteQuestionnaireStatisticViewInputModel, CompleteQuestionnaireStatisticView>(
                    new CompleteQuestionnaireStatisticViewInputModel(id));
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
                return this.RedirectToAction("Assigments", new { id = model.TemplateId });
            if (ModelState.IsValid)
            {
                var commandService = NcqrsEnvironment.Get<ICommandService>();
                var status = SurveyStatus.Redo;
                status.ChangeComment = model.Comment;
                commandService.Execute(new ChangeStatusCommand() { CompleteQuestionnaireId = model.Id, Status = status, Responsible = this.globalInfo.GetCurrentUser() });
                return this.RedirectToAction("Assigments", new { id = model.TemplateId });
            }

            var stat = this.viewRepository.Load<CompleteQuestionnaireStatisticViewInputModel, CompleteQuestionnaireStatisticView>(
                    new CompleteQuestionnaireStatisticViewInputModel(model.Id));
            return this.View(new ApproveRedoModel() { Id = model.Id, Statistic = stat, TemplateId = model.TemplateId });
        }

        /// <summary>
        /// Action for preparing data for visual chart
        /// </summary>
        /// <param name="templateId">
        /// The template Id.
        /// </param>
        /// <returns>
        /// return Partial View with visual chart
        /// </returns>
        public ActionResult Chart(Guid templateId)
        {
            var data = new ChartDataModel("Chart");
            var view = this.viewRepository.Load<AssignmentInputModel, AssignmentView>(new AssignmentInputModel(templateId, Guid.Empty, 1, 100, new List<OrderRequestItem>()));
            if (view.Items.Count > 0)
            {
                if (view.Items.Where(t => t.Responsible == null).Count() > 0)
                {
                    data.Data.Add("Unassigned", view.Items.Where(t => t.Responsible == null).Count());
                }

                var statusesName = view.Items.Select(t => t.Status.Name).Distinct().ToList();
                foreach (var state in statusesName)
                {
                    data.Data.Add(state, view.Items.Where(t => t.Status.Name == state).Count());
                }
            }

            return this.PartialView(data);
        }

        public ActionResult Administration()
        {
            return this.View();
        }

        #endregion

    }
}