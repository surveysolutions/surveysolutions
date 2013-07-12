using WB.Core.GenericSubdomains.Logging;

namespace Web.Supervisor.Controllers
{
    using System;
    using System.Collections.Generic;
    using System.Web.Mvc;

    using Core.Supervisor.Views.Survey;

    using Main.Core.Commands.Questionnaire.Completed;
    using Main.Core.Entities.SubEntities;
    using Main.Core.View;
    using Main.Core.View.CompleteQuestionnaire.Statistics;

    using Ncqrs.Commanding.ServiceModel;

    using Questionnaire.Core.Web.Helpers;
    using Web.Supervisor.Models;

    [Authorize(Roles = "Headquarter, Supervisor")]
    public class InterviewController : BaseController
    {
        private readonly IViewFactory<CompleteQuestionnaireStatisticViewInputModel, CompleteQuestionnaireStatisticView> completeQuestionnaireStatisticViewFactory;
        private readonly IViewFactory<DisplayViewInputModel, SurveyScreenView> surveyScreenViewFactory;

        public InterviewController(ICommandService commandService, IGlobalInfoProvider provider, ILogger logger,
            IViewFactory<CompleteQuestionnaireStatisticViewInputModel, CompleteQuestionnaireStatisticView> completeQuestionnaireStatisticViewFactory,
            IViewFactory<DisplayViewInputModel, SurveyScreenView> surveyScreenViewFactory)
            : base(commandService, provider, logger)
        {
            this.completeQuestionnaireStatisticViewFactory = completeQuestionnaireStatisticViewFactory;
            this.surveyScreenViewFactory = surveyScreenViewFactory;
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
                Logger.Fatal("Unexpected error occurred", e);
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
                Logger.Fatal("Unexpected error occurred", e);
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
                Logger.Fatal("Unexpected error occurred", e);
                return this.Json(new { status = "error", error = e.Message });
            }

            return this.Json(new { status = "ok" });
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

        public ActionResult ChangeState(Guid id, string template)
        {
            var stat = this.completeQuestionnaireStatisticViewFactory.Load(
         new CompleteQuestionnaireStatisticViewInputModel(id) { Scope = QuestionScope.Supervisor });
            return this.View(new ApproveRedoModel() { Id = id, Statistic = stat, TemplateId = template });
        }

        [HttpPost]
        public ActionResult ChangeState(ApproveRedoModel model, int state, string cancel)
        {
            if (state == 2)
            {
                if (cancel != null)
                    return this.RedirectToAction("Index", new { id = model.TemplateId });
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
                    return this.RedirectToAction("Index", new { id = model.TemplateId });
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
                    return this.RedirectToAction("Index", new { id = model.TemplateId });
                }

                var stat = this.completeQuestionnaireStatisticViewFactory.Load(
                        new CompleteQuestionnaireStatisticViewInputModel(model.Id) { Scope = QuestionScope.Supervisor });
                return this.View(new ApproveRedoModel() { Id = model.Id, Statistic = stat, TemplateId = model.TemplateId });
            }
        }

        public ActionResult StatusHistory(Guid id)
        {
            var stat = this.completeQuestionnaireStatisticViewFactory.Load(
         new CompleteQuestionnaireStatisticViewInputModel(id) { Scope = QuestionScope.Supervisor });
            return this.PartialView("_StatusHistory", stat.StatusHistory);
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
                return this.RedirectToAction("Index", new { id = model.TemplateId });
            }

            var stat = this.completeQuestionnaireStatisticViewFactory.Load(
                    new CompleteQuestionnaireStatisticViewInputModel(model.Id) { Scope = QuestionScope.Supervisor });
            return this.View(new ApproveRedoModel() { Id = model.Id, Statistic = stat, TemplateId = model.TemplateId });
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
                return this.RedirectToAction("Index", new { id = model.TemplateId });
            if (ModelState.IsValid)
            {
                var status = SurveyStatus.Redo;
                status.ChangeComment = model.Comment;
                this.CommandService.Execute(new ChangeStatusCommand() { CompleteQuestionnaireId = model.Id, Status = status, Responsible = this.GlobalInfo.GetCurrentUser() });
                return this.RedirectToAction("Index", new { id = model.TemplateId });
            }

            var stat = this.completeQuestionnaireStatisticViewFactory.Load(
                    new CompleteQuestionnaireStatisticViewInputModel(model.Id) { Scope = QuestionScope.Supervisor });
            return this.View(new ApproveRedoModel() { Id = model.Id, Statistic = stat, TemplateId = model.TemplateId });
        }
    }
}