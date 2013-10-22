using System;
using System.Web.Mvc;
using System.Web.Security;
using Core.Supervisor.Views.ChangeStatus;
using Core.Supervisor.Views.Interview;
using Core.Supervisor.Views.Survey;
using Main.Core.Entities.SubEntities;
using Main.Core.View;
using Ncqrs.Commanding.ServiceModel;
using Questionnaire.Core.Web.Helpers;
using WB.Core.GenericSubdomains.Logging;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;
using Web.Supervisor.Code;
using Web.Supervisor.Models;

namespace Web.Supervisor.Controllers
{
    [Authorize(Roles = "Headquarter, Supervisor")]
    public class InterviewController : BaseController
    {
        private readonly IViewFactory<ChangeStatusInputModel, ChangeStatusView> changeStatusFactory;

        private readonly IViewFactory<InterviewDetailsInputModel, InterviewDetailsView> interviewDetailsFactory;

        public InterviewController(ICommandService commandService, IGlobalInfoProvider provider, ILogger logger,
                                   IViewFactory<ChangeStatusInputModel, ChangeStatusView> changeStatusFactory,

                                   IViewFactory<InterviewDetailsInputModel, InterviewDetailsView> interviewDetailsFactory)
            : base(commandService, provider, logger)
        {
            this.changeStatusFactory = changeStatusFactory;

            this.interviewDetailsFactory = interviewDetailsFactory;
        }

        public ActionResult InterviewDetails(Guid id, string template, Guid? group, Guid? question, Guid? propagationKey)
        {
            this.ViewBag.ActivePage = MenuItem.Docs;
            InterviewDetailsView model = this.interviewDetailsFactory.Load(
                new InterviewDetailsInputModel()
                {
                    CompleteQuestionnaireId = id,
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

        public ActionResult ChangeState(Guid id)
        {
            if (this.GlobalInfo.IsHeadquarter)
            {
                return new HttpForbiddenResult("Only supervisors have access to page for interview change state");
            }

            ChangeStatusView model = this.changeStatusFactory.Load(new ChangeStatusInputModel {InterviewId = id});

            if (model.Status != InterviewStatus.Completed)
            {
                return new HttpForbiddenResult("You can change state for interviews with status \"Complete\" only");
            }

            UserLight currentUser = this.GlobalInfo.GetCurrentUser();
            this.ViewBag.CurrentUser = new UsersViewItem {UserId = currentUser.Id, UserName = currentUser.Name};
            return this.View(model);
        }

        private ActionResult RedirectToInterviewList(string templateId)
        {
            var query = new {id = templateId};
            return this.GlobalInfo.IsHeadquarter
                       ? this.RedirectToAction("Interviews", "HQ", query)
                       : this.RedirectToAction("Interviews", "Survey", query);
        }
    }
}