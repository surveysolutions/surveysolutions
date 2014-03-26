using System;
using System.Web.Mvc;
using Core.Supervisor.Views.ChangeStatus;
using Core.Supervisor.Views.Revalidate;
using Main.Core.View;
using Ncqrs.Commanding.ServiceModel;
using Questionnaire.Core.Web.Helpers;
using WB.Core.GenericSubdomains.Logging;
using WB.Core.SharedKernels.DataCollection.Commands.Interview;
using Web.Supervisor.Models;

namespace Web.Supervisor.Controllers
{
    [Authorize(Roles = "Headquarter, Supervisor")]
    public class InterviewController : BaseController
    {
        private readonly IViewFactory<ChangeStatusInputModel, ChangeStatusView> changeStatusFactory;
        private readonly IViewFactory<InterviewInfoForRevalidationInputModel, InterviewInfoForRevalidationView> revalidateInterviewViewFactory;

        public InterviewController(ICommandService commandService, IGlobalInfoProvider provider, ILogger logger,
                                   IViewFactory<ChangeStatusInputModel, ChangeStatusView> changeStatusFactory,
                                    IViewFactory<InterviewInfoForRevalidationInputModel, InterviewInfoForRevalidationView> revalidateInterviewViewFactory)
            : base(commandService, provider, logger)
        {
            this.changeStatusFactory = changeStatusFactory;
            this.revalidateInterviewViewFactory = revalidateInterviewViewFactory;
        }

        public ActionResult InterviewDetails(Guid id, string template, Guid? group, Guid? question, Guid? propagationKey)
        {
            this.ViewBag.ActivePage = MenuItem.Docs;

            ChangeStatusView interviewInfo = this.changeStatusFactory.Load(new ChangeStatusInputModel() {InterviewId = id});
            if (interviewInfo == null)
            {
                return this.RedirectToInterviewList(template);
            }

            return
                this.View(new InterviewModel()
                {
                    InterviewId = id,
                    CurrentGroupId = group,
                    CurrentPropagationKeyId = propagationKey,
                    InterviewStatus = interviewInfo.Status
                });
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