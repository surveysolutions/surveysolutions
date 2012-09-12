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
    using System.Linq;
    using System.Web.Mvc;
    using Ncqrs;
    using Ncqrs.Commanding.ServiceModel;
    using Questionnaire.Core.Web.Helpers;
    using RavenQuestionnaire.Core;
    using RavenQuestionnaire.Core.Commands.Questionnaire.Completed;
    using RavenQuestionnaire.Core.Entities.SubEntities;
    using RavenQuestionnaire.Core.Views.Assign;
    using RavenQuestionnaire.Core.Views.CompleteQuestionnaire;
    using RavenQuestionnaire.Core.Views.CompleteQuestionnaire.Mobile;
    using RavenQuestionnaire.Core.Views.Question;
    using RavenQuestionnaire.Core.Views.Statistics;
    using RavenQuestionnaire.Core.Views.Survey;
    using RavenQuestionnaire.Core.Views.User;
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
        public ActionResult Index(SurveyViewInputModel input)
        {
            var model = this.viewRepository.Load<SurveyViewInputModel, SurveyBrowseView>(input);
            return this.View(model);
        }

        /// <summary>
        /// Display Assigments statistic
        /// </summary>
        /// <param name="id">
        /// The id.
        /// </param>
        /// <param name="input">
        /// The input.
        /// </param>
        /// <param name="status">
        /// The status.
        /// </param>
        /// <returns>
        /// Return Assigments page
        /// </returns>
        public ActionResult Assigments(Guid id, SurveyGroupInputModel input, string status)
        {
            var inputModel = input == null
                                 ? new SurveyGroupInputModel() { Id = id, StatusName = status }
                                 : new SurveyGroupInputModel(id, input.Page, input.PageSize, input.Orders, status);
            var user = this.globalInfo.GetCurrentUser();
            var model = this.viewRepository.Load<SurveyGroupInputModel, SurveyGroupView>(inputModel);
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
            var user = this.globalInfo.GetCurrentUser();
            var users = this.viewRepository.Load<InterviewersInputModel, InterviewersView>(new InterviewersInputModel { Supervisor = user });
            var model = this.viewRepository.Load<AssignSurveyInputModel, AssignSurveyView>(new AssignSurveyInputModel(id));
            var r = users.Items.ToList();
            r.Insert(0, new InterviewersItem(Guid.Empty, string.Empty, string.Empty, DateTime.MinValue, false, 0, 0, 0));
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
                    new CompleteQuestionnaireStatisticViewInputModel(id.ToString()));
            return this.View(new ApproveModel() { Id = id, Statistic = stat, TemplateId = template });
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
        public ActionResult Approve(ApproveModel model)
        {
            if (ModelState.IsValid)
            {
                var commandService = NcqrsEnvironment.Get<ICommandService>();
                var status = SurveyStatus.Approve;
                status.ChangeComment = model.Comment;
                commandService.Execute(new ChangeStatusCommand() { CompleteQuestionnaireId = model.Id, Status = status });
                return this.RedirectToAction("Assigments", new { id = model.TemplateId });
            }

            var stat = this.viewRepository.Load<CompleteQuestionnaireStatisticViewInputModel, CompleteQuestionnaireStatisticView>(
                    new CompleteQuestionnaireStatisticViewInputModel(model.Id.ToString()));
            return this.View(new ApproveModel() { Id = model.Id, Statistic = stat, TemplateId = model.TemplateId });
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
            var model = this.viewRepository.Load<CompleteQuestionnaireViewInputModel, CompleteQuestionnaireMobileView>(
                new CompleteQuestionnaireViewInputModel(id) { CurrentGroupPublicKey = group, PropagationKey = propagationKey });
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
            var model = this.viewRepository.Load<CompleteQuestionnaireViewInputModel, CompleteGroupMobileView>(
                new CompleteQuestionnaireViewInputModel(id, group, propagationKey));
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
                return Json(new { status = "ok", userId = responsible.Id, userName = responsible.Name, cqId = cqId }, JsonRequestBehavior.AllowGet);
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
            try
            {
                var commandService = NcqrsEnvironment.Get<ICommandService>();
                commandService.Execute(new SetAnswerCommand(Guid.Parse(settings[0].QuestionnaireId), question, settings[0].PropogationPublicKey));
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
            var input = new SurveyViewInputModel
                            {
                                Page = data.Pager.Page,
                                PageSize = data.Pager.PageSize,
                                Orders = data.SortOrder
                            };
            var model = this.viewRepository.Load<SurveyViewInputModel, SurveyBrowseView>(input);
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
            var input = new SurveyGroupInputModel(data.Id, data.Pager.Page, data.Pager.PageSize, data.SortOrder);
            var model = this.viewRepository.Load<SurveyGroupInputModel, SurveyGroupView>(input);
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
            var model = this.viewRepository.Load<CompleteQuestionnaireViewInputModel, CompleteQuestionnaireMobileView>(
                new CompleteQuestionnaireViewInputModel(id));
            ViewBag.TemplateId = template;
            return this.View("Comments", model);
        }

        #endregion
    }
}