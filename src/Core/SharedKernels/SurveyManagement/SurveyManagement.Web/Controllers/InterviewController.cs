using System;
using System.Reflection;
using System.Web.Mvc;
using Main.Core.View;
using Ncqrs.Commanding.ServiceModel;
using WB.Core.GenericSubdomains.Logging;
using WB.Core.SharedKernels.DataCollection.Commands.Interview;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.SurveyManagement.Views.ChangeStatus;
using WB.Core.SharedKernels.SurveyManagement.Views.Interview;
using WB.Core.SharedKernels.SurveyManagement.Views.InterviewHistory;
using WB.Core.SharedKernels.SurveyManagement.Views.Revalidate;
using WB.Core.SharedKernels.SurveyManagement.Web.Models;
using WB.Core.SharedKernels.SurveyManagement.Web.Utils.Membership;

namespace WB.Core.SharedKernels.SurveyManagement.Web.Controllers
{
    [Authorize(Roles = "Headquarter, Supervisor")]
    public class InterviewController : BaseController
    {
        private readonly IViewFactory<ChangeStatusInputModel, ChangeStatusView> changeStatusFactory;
        private readonly IViewFactory<InterviewInfoForRevalidationInputModel, InterviewInfoForRevalidationView> revalidateInterviewViewFactory;
        private readonly IViewFactory<InterviewHistoryInputModel, InterviewHistoryView> interviewHistoryViewFactory;
        private readonly IInterviewSummaryViewFactory interviewSummaryViewFactory;
        private readonly IPlainFileRepository plainFileRepository;

        public InterviewController(ICommandService commandService, IGlobalInfoProvider provider, ILogger logger,
            IViewFactory<ChangeStatusInputModel, ChangeStatusView> changeStatusFactory,
            IViewFactory<InterviewInfoForRevalidationInputModel, InterviewInfoForRevalidationView> revalidateInterviewViewFactory,
            IInterviewSummaryViewFactory interviewSummaryViewFactory, IViewFactory<InterviewHistoryInputModel, InterviewHistoryView> interviewHistoryViewFactory, IPlainFileRepository plainFileRepository)
            : base(commandService, provider, logger)
        {
            this.changeStatusFactory = changeStatusFactory;
            this.revalidateInterviewViewFactory = revalidateInterviewViewFactory;
            this.interviewSummaryViewFactory = interviewSummaryViewFactory;
            this.interviewHistoryViewFactory = interviewHistoryViewFactory;
            this.plainFileRepository = plainFileRepository;
        }

        public ActionResult InterviewDetails(Guid id, string template, Guid? group, Guid? question, Guid? propagationKey)
        {
            this.ViewBag.ActivePage = MenuItem.Docs;

            ChangeStatusView interviewInfo = this.changeStatusFactory.Load(new ChangeStatusInputModel() {InterviewId = id});
            InterviewSummary interviewSummary = this.interviewSummaryViewFactory.Load(id);
            
            if (interviewInfo == null || interviewSummary == null)
                return HttpNotFound();

            bool isAccessAllowed =
                this.GlobalInfo.IsHeadquarter ||
                (this.GlobalInfo.IsSurepvisor && this.GlobalInfo.GetCurrentUser().Id == interviewSummary.TeamLeadId);

            if (!isAccessAllowed)
                return HttpNotFound();

            return
                this.View(new InterviewModel()
                {
                    InterviewId = id,
                    CurrentGroupId = group,
                    CurrentPropagationKeyId = propagationKey,
                    InterviewStatus = interviewInfo.Status
                });
        }

        public ActionResult InterviewFile(Guid interviewId, string fileName)
        {
            var file = plainFileRepository.GetInterviewBinaryData(interviewId, fileName);
            if(file==null)
                return this.File(Assembly.GetExecutingAssembly().GetManifestResourceStream("WB.Core.SharedKernels.SurveyManagement.Web.Content.img.no_image_found.jpg"), "image/jpeg", fileName);
            return this.File(file, "image/jpeg", fileName);
        }

        public ActionResult InterviewHistory(Guid id)
        {
            return this.View(interviewHistoryViewFactory.Load(new InterviewHistoryInputModel(id)));
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
            return this.RedirectToAction("ConfirmRevalidation", new { id = input.InterviewId });
        }

        [Authorize(Roles = "Headquarter, Supervisor")]
        public ActionResult ConfirmRevalidation(Guid id)
        {
            var model = this.revalidateInterviewViewFactory.Load(new InterviewInfoForRevalidationInputModel { InterviewId = id });
            return this.View(model);
        }

        [Authorize(Roles = "Headquarter, Supervisor")]
        [HttpPost]
        public ActionResult ConfirmRevalidation(RevalidateModel input)
        {
            this.CommandService.Execute(new ReevaluateSynchronizedInterview(input.InterviewId));
            var model = this.revalidateInterviewViewFactory.Load(new InterviewInfoForRevalidationInputModel { InterviewId = input.InterviewId });
            return this.View("ConfirmRevalidation", model);
        }
    }
}