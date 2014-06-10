using System;
using System.Web.Mvc;
using Main.Core.View;
using Ncqrs.Commanding.ServiceModel;
using Questionnaire.Core.Web.Helpers;
using WB.Core.GenericSubdomains.Logging;
using WB.Core.SharedKernels.DataCollection.Commands.Interview;
using WB.Core.SharedKernels.SurveyManagement.Views.ChangeStatus;
using WB.Core.SharedKernels.SurveyManagement.Views.Revalidate;
using WB.Core.SharedKernels.SurveyManagement.Web.Models;

namespace WB.Core.SharedKernels.SurveyManagement.Web.Controllers
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
                return HttpNotFound();
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

        [Authorize(Roles = "Headquarter, Supervisor")]
        public ActionResult Revalidate()
        {
            return this.View(new RevalidateModel());
        }

        [Authorize(Roles = "Headquarter, Supervisor")]
        [HttpPost]
        public ActionResult Revalidate(RevalidateModel input)
        {
            return this.RedirectToAction("ConfirmInterviewRevalidation", new {id = input.InterviewId });
        }

        [Authorize(Roles = "Headquarter, Supervisor")]
        public ActionResult ConfirmRevalidation(Guid id)
        {
            var model = this.revalidateInterviewViewFactory.Load(new InterviewInfoForRevalidationInputModel { InterviewId = id });
            return this.View(model);
        }

        [Authorize(Roles = "Headquarter, Supervisor")]
        [HttpPost]
        public ActionResult ConfirmInterviewRevalidation(Guid interviewId)
        {
            this.CommandService.Execute(new ReevaluateSynchronizedInterview(interviewId));
            var model = this.revalidateInterviewViewFactory.Load(new InterviewInfoForRevalidationInputModel { InterviewId = interviewId });
            return this.View("ConfirmRevalidation", model);
        }
    }
}