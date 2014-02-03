using System;
using System.Web.Mvc;
using System.Web.Security;
using Core.Supervisor.Views.ChangeStatus;
using Core.Supervisor.Views.Interview;
using Core.Supervisor.Views.Revalidate;
using Core.Supervisor.Views.Survey;
using Main.Core.Entities.SubEntities;
using Main.Core.View;
using Ncqrs.Commanding.ServiceModel;
using Questionnaire.Core.Web.Helpers;
using WB.Core.GenericSubdomains.Logging;
using WB.Core.SharedKernels.DataCollection.Commands.Interview;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;
using Web.Supervisor.Code;
using Web.Supervisor.Models;

namespace Web.Supervisor.Controllers
{
    [Authorize(Roles = "Headquarter, Supervisor")]
    public class InterviewController : BaseController
    {
        private readonly IViewFactory<ChangeStatusInputModel, ChangeStatusView> changeStatusFactory;
        private readonly IViewFactory<InterviewInfoForRevalidationInputModel, InterviewInfoForRevalidationView> revalidateInterviewViewFactory;
        private readonly IViewFactory<InterviewDetailsInputModel, InterviewDetailsView> interviewDetailsFactory;

        public InterviewController(ICommandService commandService, IGlobalInfoProvider provider, ILogger logger,
                                   IViewFactory<ChangeStatusInputModel, ChangeStatusView> changeStatusFactory,
                                    IViewFactory<InterviewInfoForRevalidationInputModel, InterviewInfoForRevalidationView> revalidateInterviewViewFactory,
                                   IViewFactory<InterviewDetailsInputModel, InterviewDetailsView> interviewDetailsFactory)
            : base(commandService, provider, logger)
        {
            this.changeStatusFactory = changeStatusFactory;
            this.revalidateInterviewViewFactory = revalidateInterviewViewFactory;

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

        [Authorize(Roles = "Headquarter")]
        public ActionResult Revalidate()
        {
            return this.View(new RevalidateModel());
        }

        [Authorize(Roles = "Headquarter")]
        [HttpPost]
        public ActionResult Revalidate(RevalidateModel input)
        {
            return this.RedirectToAction("ConfirmInterviewRevalidation", new {id = input.InterviewId });
        }

        [Authorize(Roles = "Headquarter")]
        public ActionResult ConfirmRevalidation(Guid id)
        {
            var model = this.revalidateInterviewViewFactory.Load(new InterviewInfoForRevalidationInputModel { InterviewId = id });
            return this.View(model);
        }

        [Authorize(Roles = "Headquarter")]
        [HttpPost]
        public ActionResult ConfirmInterviewRevalidation(Guid interviewId)
        {
            this.CommandService.Execute(new ReevaluateSynchronizedInterview(interviewId));
            var model = this.revalidateInterviewViewFactory.Load(new InterviewInfoForRevalidationInputModel { InterviewId = interviewId });
            return this.View("ConfirmRevalidation", model);
        }

        private ActionResult RedirectToInterviewList(string templateId)
        {
            var query = new { id = templateId };
            return this.GlobalInfo.IsHeadquarter
                       ? this.RedirectToAction("Interviews", "HQ", query)
                       : this.RedirectToAction("Interviews", "Survey", query);
        }


    }
}