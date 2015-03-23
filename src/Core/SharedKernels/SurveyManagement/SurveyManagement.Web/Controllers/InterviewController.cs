using System;
using System.Linq;
using System.Web.Mvc;
using WB.Core.GenericSubdomains.Utils.Services;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.Infrastructure.ReadSide;
using WB.Core.SharedKernels.DataCollection.Commands.Interview;
using WB.Core.SharedKernels.SurveyManagement.Views;
using WB.Core.SharedKernels.SurveyManagement.Views.ChangeStatus;
using WB.Core.SharedKernels.SurveyManagement.Views.Interview;
using WB.Core.SharedKernels.SurveyManagement.Views.InterviewHistory;
using WB.Core.SharedKernels.SurveyManagement.Views.Revalidate;
using WB.Core.SharedKernels.SurveyManagement.Web.Models;
using WB.Core.SharedKernels.SurveyManagement.Web.Utils.Membership;

namespace WB.Core.SharedKernels.SurveyManagement.Web.Controllers
{
    [Authorize(Roles = "Administrator, Headquarter, Supervisor")]
    public class InterviewController : BaseController
    {
        private readonly IViewFactory<ChangeStatusInputModel, ChangeStatusView> changeStatusFactory;
        private readonly IViewFactory<InterviewInfoForRevalidationInputModel, InterviewInfoForRevalidationView> revalidateInterviewViewFactory;
        private readonly IInterviewHistoryFactory interviewHistoryViewFactory;
        private readonly IInterviewSummaryViewFactory interviewSummaryViewFactory;
        private readonly IInterviewDetailsViewFactory interviewDetailsViewFactory;

        public InterviewController(
            ICommandService commandService, 
            IGlobalInfoProvider provider, 
            ILogger logger,
            IViewFactory<ChangeStatusInputModel, ChangeStatusView> changeStatusFactory,
            IViewFactory<InterviewInfoForRevalidationInputModel, InterviewInfoForRevalidationView> revalidateInterviewViewFactory,
            IInterviewSummaryViewFactory interviewSummaryViewFactory,
            IInterviewHistoryFactory interviewHistoryViewFactory, 
            IInterviewDetailsViewFactory interviewDetailsViewFactory)
            : base(commandService, provider, logger)
        {
            this.changeStatusFactory = changeStatusFactory;
            this.revalidateInterviewViewFactory = revalidateInterviewViewFactory;
            this.interviewSummaryViewFactory = interviewSummaryViewFactory;
            this.interviewHistoryViewFactory = interviewHistoryViewFactory;
            this.interviewDetailsViewFactory = interviewDetailsViewFactory;
        }

        private decimal[] ParseRosterVector(string rosterVectorAsString)
        {
            if (string.IsNullOrEmpty(rosterVectorAsString))
                return new decimal[0];

            return rosterVectorAsString.Split('_').Select(vector => decimal.Parse(vector.Replace('-', '.'))).ToArray();
        }

        public ActionResult Details(Guid id, InterviewDetailsFilter? filter, Guid? currentGroupId, string rosterVector)
        {
            if (!filter.HasValue)
                return this.RedirectToAction("Details",
                    new
                    {
                        id = id,
                        filter = InterviewDetailsFilter.All,
                        currentGroupId = this.interviewDetailsViewFactory.GetFirstChapterId(id)
                    });

            this.ViewBag.ActivePage = MenuItem.Docs;

            InterviewSummary interviewSummary = this.interviewSummaryViewFactory.Load(id);

            bool isAccessAllowed =
                this.GlobalInfo.IsHeadquarter || this.GlobalInfo.IsAdministrator ||
                (this.GlobalInfo.IsSurepvisor && this.GlobalInfo.GetCurrentUser().Id == interviewSummary.TeamLeadId);

            if (!isAccessAllowed)
                return HttpNotFound();

            ChangeStatusView interviewInfo =
                this.changeStatusFactory.Load(new ChangeStatusInputModel() {InterviewId = id});

            if (interviewInfo == null || interviewSummary == null)
                return HttpNotFound();

            return
                View(interviewDetailsViewFactory.GetInterviewDetails(interviewId: id, currentGroupId: currentGroupId,
                    filter: filter, currentGroupRosterVector: this.ParseRosterVector(rosterVector)));
        }

        public ActionResult InterviewHistory(Guid id)
        {
            return this.View(interviewHistoryViewFactory.Load(id));
        }

        [Authorize(Roles = "Administrator, Headquarter, Supervisor")]
        public ActionResult Revalidate()
        {
            return this.View(new RevalidateModel());
        }

        [Authorize(Roles = "Administrator, Headquarter, Supervisor")]
        [HttpPost]
        public ActionResult Revalidate(RevalidateModel input)
        {
            return this.RedirectToAction("ConfirmRevalidation", new { id = input.InterviewId });
        }

        [Authorize(Roles = "Administrator, Headquarter, Supervisor")]
        public ActionResult ConfirmRevalidation(Guid id)
        {
            var model = this.revalidateInterviewViewFactory.Load(new InterviewInfoForRevalidationInputModel { InterviewId = id });
            return this.View(model);
        }

        [Authorize(Roles = "Administrator, Headquarter, Supervisor")]
        [HttpPost]
        public ActionResult ConfirmRevalidation(RevalidateModel input)
        {
            this.CommandService.Execute(new ReevaluateSynchronizedInterview(input.InterviewId));
            var model = this.revalidateInterviewViewFactory.Load(new InterviewInfoForRevalidationInputModel { InterviewId = input.InterviewId });
            return this.View("ConfirmRevalidation", model);
        }
    }
}