using System;
using System.Linq;
using System.Web.Mvc;
using WB.Core.BoundedContexts.Headquarters.Services;
using WB.Core.BoundedContexts.Headquarters.Views;
using WB.Core.BoundedContexts.Headquarters.Views.ChangeStatus;
using WB.Core.BoundedContexts.Headquarters.Views.Interview;
using WB.Core.BoundedContexts.Headquarters.Views.InterviewHistory;
using WB.Core.BoundedContexts.Headquarters.Views.Revalidate;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.SharedKernels.DataCollection.Commands.Interview;
using WB.Core.SharedKernels.SurveyManagement.Web.Filters;
using WB.Core.SharedKernels.SurveyManagement.Web.Models;
using WB.Core.SharedKernels.SurveyManagement.Web.Utils.Membership;
using WB.UI.Headquarters.Controllers;
using WB.UI.Headquarters.Resources;

namespace WB.Core.SharedKernels.SurveyManagement.Web.Controllers
{
    [LimitsFilter]
    [Authorize(Roles = "Administrator, Headquarter, Supervisor")]
    public class InterviewController : BaseController
    {
        private readonly IChangeStatusFactory changeStatusFactory;
        private readonly IInterviewTroubleshootFactory troubleshootInterviewViewFactory;
        private readonly IInterviewHistoryFactory interviewHistoryViewFactory;
        private readonly IInterviewSummaryViewFactory interviewSummaryViewFactory;
        private readonly IInterviewDetailsViewFactory interviewDetailsViewFactory;

        public InterviewController(
            ICommandService commandService, 
            IGlobalInfoProvider provider, 
            ILogger logger,
            IChangeStatusFactory changeStatusFactory,
            IInterviewTroubleshootFactory troubleshootInterviewViewFactory,
            IInterviewSummaryViewFactory interviewSummaryViewFactory,
            IInterviewHistoryFactory interviewHistoryViewFactory, 
            IInterviewDetailsViewFactory interviewDetailsViewFactory)
            : base(commandService, provider, logger)
        {
            this.changeStatusFactory = changeStatusFactory;
            this.troubleshootInterviewViewFactory = troubleshootInterviewViewFactory;
            this.interviewSummaryViewFactory = interviewSummaryViewFactory;
            this.interviewHistoryViewFactory = interviewHistoryViewFactory;
            this.interviewDetailsViewFactory = interviewDetailsViewFactory;
        }

        private decimal[] ParseRosterVector(string rosterVectorAsString)
        {
            if (string.IsNullOrEmpty(rosterVectorAsString))
                return new decimal[0];

            return rosterVectorAsString.Split('_').Select(vector => decimal.Parse(vector)).ToArray();
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
            this.ViewBag.InterviewId = id;

            InterviewSummary interviewSummary = this.interviewSummaryViewFactory.Load(id);

            bool isAccessAllowed =
                this.GlobalInfo.IsHeadquarter || this.GlobalInfo.IsAdministrator ||
                (this.GlobalInfo.IsSupervisor && this.GlobalInfo.GetCurrentUser().Id == interviewSummary.TeamLeadId);

            if (!isAccessAllowed)
                return HttpNotFound();

            ChangeStatusView interviewInfo =
                this.changeStatusFactory.Load(new ChangeStatusInputModel {InterviewId = id});

            if (interviewInfo == null || interviewSummary == null)
                return HttpNotFound();

            var detailsViewModel = interviewDetailsViewFactory.GetInterviewDetails(interviewId: id, 
                                                                                   currentGroupId: currentGroupId,
                                                                                   filter: filter, 
                                                                                   currentGroupRosterVector: this.ParseRosterVector(rosterVector));
            return
                View(detailsViewModel);
        }

        public ActionResult InterviewHistory(Guid id)
        {
            return this.View(interviewHistoryViewFactory.Load(id));
        }

        [Authorize(Roles = "Administrator, Headquarter, Supervisor")]
        public ActionResult Troubleshoot()
        {
            return this.View(new TroubleshootModel());
        }
        
        [Authorize(Roles = "Administrator, Headquarter, Supervisor")]
        [HttpPost]
        public ActionResult Troubleshoot(TroubleshootModel input)
        {
            if (this.ModelState.IsValid)
            {
                return this.RedirectToAction("Troubleshooting", new { id = input.InterviewId });
            }

            return this.View(input);
        }

        [Authorize(Roles = "Administrator, Headquarter, Supervisor")]
        public ActionResult Troubleshooting(Guid id)
        {
            try
            {
                InterviewTroubleshootView model = this.troubleshootInterviewViewFactory.Load(new InterviewTroubleshootInputModel { InterviewId = id });
                return this.View(model);
            }
            catch (Exception ex)
            {
                this.Logger.Error("Error on Troubleshooting", ex);

                this.Error(Strings.UnexpectedErrorOccurred);
                return this.RedirectToAction("Troubleshoot", new TroubleshootModel { InterviewId = id });
            }
        }

        [Authorize(Roles = "Administrator, Headquarter, Supervisor")]
        [HttpPost]
        [ObserverNotAllowed]
        public ActionResult Revalidate(TroubleshootModel input)
        {
            try
            {
                this.CommandService.Execute(new ReevaluateSynchronizedInterview(input.InterviewId));
            }
            catch (Exception exception)
            {
                Logger.Error(exception.Message, exception);
                this.TempData["Revalidation.Error"] = Pages.InterviewController_RevalidationFailed;
                this.TempData["Revalidation.ErrorDetails"] = exception.Message;
            }
            var inputModel = new InterviewTroubleshootInputModel { InterviewId = input.InterviewId };

            var newModel = this.troubleshootInterviewViewFactory.Load(inputModel);

            return this.View("Troubleshooting", newModel);
        }

        [Authorize(Roles = "Administrator, Headquarter, Supervisor")]
        [HttpPost]
        [ObserverNotAllowed]
        public ActionResult RepeatLastStatus(TroubleshootModel input)
        {
            try
            {
                this.CommandService.Execute(new RepeatLastInterviewStatus(input.InterviewId, Pages.InterviewController_RepeatLastStatus));
            }
            catch (Exception exception)
            {
                Logger.Error(exception.Message, exception);
                this.TempData["RepeatLastStatus.Error"] = Pages.InterviewController_RepeatLastStatusFailed;
                this.TempData["RepeatLastStatus.ErrorDetails"] = exception.Message;
            }
            var inputModel = new InterviewTroubleshootInputModel { InterviewId = input.InterviewId };

            var newModel = this.troubleshootInterviewViewFactory.Load(inputModel);

            return this.View("Troubleshooting", newModel);
        }
    }
}