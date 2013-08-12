using System;
using System.Web.Mvc;
using Core.Supervisor.Views.Survey;
using Main.Core.Entities.SubEntities;
using Main.Core.View;
using Main.Core.View.CompleteQuestionnaire.Statistics;
using Ncqrs.Commanding.ServiceModel;
using Questionnaire.Core.Web.Helpers;
using WB.Core.GenericSubdomains.Logging;
using Web.Supervisor.Models;

namespace Web.Supervisor.Controllers
{
    [Authorize(Roles = "Headquarter, Supervisor")]
    public class InterviewController : BaseController
    {
        private readonly IViewFactory<CompleteQuestionnaireStatisticViewInputModel, CompleteQuestionnaireStatisticView>
            completeQuestionnaireStatisticViewFactory;

        private readonly IViewFactory<DisplayViewInputModel, SurveyScreenView> surveyScreenViewFactory;

        public InterviewController(ICommandService commandService, IGlobalInfoProvider provider, ILogger logger,
                                   IViewFactory<CompleteQuestionnaireStatisticViewInputModel, CompleteQuestionnaireStatisticView>
                                       completeQuestionnaireStatisticViewFactory,
                                   IViewFactory<DisplayViewInputModel, SurveyScreenView> surveyScreenViewFactory)
            : base(commandService, provider, logger)
        {
            this.completeQuestionnaireStatisticViewFactory = completeQuestionnaireStatisticViewFactory;
            this.surveyScreenViewFactory = surveyScreenViewFactory;
        }

        public ActionResult Details(Guid id, string template, Guid? group, Guid? question, Guid? propagationKey)
        {
            this.ViewBag.ActivePage = MenuItem.Docs;
            SurveyScreenView model = this.surveyScreenViewFactory.Load(
                new DisplayViewInputModel(id)
                    {
                        CurrentGroupPublicKey = group,
                        PropagationKey = propagationKey,
                        User = this.GlobalInfo.GetCurrentUser()
                    });
            if (model == null)
            {
                return this.RedirectToInterviewList(template);
            }
            this.ViewBag.CurrentQuestion = question.HasValue ? question.Value : new Guid();
            this.ViewBag.TemplateId = template;
            UserLight user = this.GlobalInfo.GetCurrentUser();
            this.ViewBag.CurrentUser = user;
            return this.View(model);
        }

        public ActionResult ChangeState(Guid id, string template)
        {
            CompleteQuestionnaireStatisticView stat = this.completeQuestionnaireStatisticViewFactory.Load(new CompleteQuestionnaireStatisticViewInputModel(id) {Scope = QuestionScope.Supervisor});
            return this.View(new ApproveRedoModel {Id = id, Statistic = stat, TemplateId = template});
        }

        public ActionResult StatusHistory(Guid id)
        {
            CompleteQuestionnaireStatisticView stat = this.completeQuestionnaireStatisticViewFactory.Load(
                new CompleteQuestionnaireStatisticViewInputModel(id) {Scope = QuestionScope.Supervisor});
            return this.PartialView("_StatusHistory", stat.StatusHistory);
        }

        private ActionResult RedirectToInterviewList(string templateId)
        {
            var query = new {id = templateId};
            return this.GlobalInfo.IsHeadquarter
                       ? this.RedirectToAction("Interviews", "HQ", query)
                       : this.RedirectToAction("Interviews", "Survey", query);
        }

        /*
      [HttpPost]
      public ActionResult ChangeState(ApproveRedoModel model, int state, string cancel)
      {
          if (state == 2)
          {
              if (cancel != null)
                  return this.RedirectToInterviewList(model.TemplateId);
              if (ModelState.IsValid)
              {
                  var status = SurveyStatus.Redo;
                  status.ChangeComment = model.Comment;
                  this.CommandService.Execute(new ChangeStatusCommand() { CompleteQuestionnaireId = model.Id, Status = status, Responsible = this.GlobalInfo.GetCurrentUser() });
                  return this.RedirectToInterviewList(model.TemplateId);
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
                  return this.RedirectToInterviewList(model.TemplateId);
              }

              var stat = this.completeQuestionnaireStatisticViewFactory.Load(
                      new CompleteQuestionnaireStatisticViewInputModel(model.Id) { Scope = QuestionScope.Supervisor });
              return this.View(new ApproveRedoModel() { Id = model.Id, Statistic = stat, TemplateId = model.TemplateId });
          }
      }
      */
    }
}