﻿using System;
using System.Linq;
using System.Web.Mvc;
using Main.Core.Entities.SubEntities;
using WB.Core.BoundedContexts.Headquarters.Services;
using WB.Core.BoundedContexts.Headquarters.Views;
using WB.Core.BoundedContexts.Headquarters.Views.ChangeStatus;
using WB.Core.BoundedContexts.Headquarters.Views.Interview;
using WB.Core.BoundedContexts.Headquarters.Views.InterviewHistory;
using WB.Core.BoundedContexts.Headquarters.Views.Revalidate;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Commands.Interview;
using WB.Core.SharedKernels.SurveyManagement.Web.Filters;
using WB.Core.SharedKernels.SurveyManagement.Web.Models;
using WB.UI.Headquarters.Controllers;
using WB.UI.Headquarters.Filters;
using WB.UI.Headquarters.Models;
using WB.UI.Headquarters.Resources;

namespace WB.Core.SharedKernels.SurveyManagement.Web.Controllers
{
    [LimitsFilter]
    [Authorize(Roles = "Administrator, Headquarter, Supervisor")]
    public class InterviewController : BaseController
    {
        private readonly IAuthorizedUser authorizedUser;
        private readonly IChangeStatusFactory changeStatusFactory;
        private readonly IInterviewTroubleshootFactory troubleshootInterviewViewFactory;
        private readonly IInterviewHistoryFactory interviewHistoryViewFactory;
        private readonly IInterviewSummaryViewFactory interviewSummaryViewFactory;
        private readonly IInterviewDetailsViewFactory interviewDetailsViewFactory;

        public InterviewController(
            ICommandService commandService, 
            IAuthorizedUser authorizedUser,
            ILogger logger,
            IChangeStatusFactory changeStatusFactory,
            IInterviewTroubleshootFactory troubleshootInterviewViewFactory,
            IInterviewSummaryViewFactory interviewSummaryViewFactory,
            IInterviewHistoryFactory interviewHistoryViewFactory, 
            IInterviewDetailsViewFactory interviewDetailsViewFactory)
            : base(commandService, logger)
        {
            this.authorizedUser = authorizedUser;
            this.changeStatusFactory = changeStatusFactory;
            this.troubleshootInterviewViewFactory = troubleshootInterviewViewFactory;
            this.interviewSummaryViewFactory = interviewSummaryViewFactory;
            this.interviewHistoryViewFactory = interviewHistoryViewFactory;
            this.interviewDetailsViewFactory = interviewDetailsViewFactory;
        }

        public ActionResult Details(Guid id, InterviewDetailsFilter? questionsTypes, string currentGroupId)
        {
            if (!questionsTypes.HasValue)
                return this.RedirectToAction("Details",
                    new
                    {
                        id = id,
                        questionsTypes = InterviewDetailsFilter.All,
                        currentGroupId = this.interviewDetailsViewFactory.GetFirstChapterId(id).FormatGuid()
                    });

            this.ViewBag.ActivePage = MenuItem.Docs;
            this.ViewBag.InterviewId = id;

            InterviewSummary interviewSummary = this.interviewSummaryViewFactory.Load(id);

            ChangeStatusView interviewInfo =
                this.changeStatusFactory.Load(new ChangeStatusInputModel { InterviewId = id });

            if (interviewInfo == null || interviewSummary == null || interviewSummary.IsDeleted)
                return HttpNotFound();

            bool isAccessAllowed =
                this.authorizedUser.IsHeadquarter || this.authorizedUser.IsAdministrator ||
                (this.authorizedUser.IsSupervisor && this.authorizedUser.Id == interviewSummary.TeamLeadId);

            if (!isAccessAllowed)
                return HttpNotFound();

            var detailsViewModel = interviewDetailsViewFactory.GetInterviewDetails(interviewId: id,
                questionsTypes: questionsTypes ?? InterviewDetailsFilter.All,
                currentGroupIdentity: string.IsNullOrEmpty(currentGroupId) ? null : Identity.Parse(currentGroupId));

            return View(detailsViewModel);
        }

        public ActionResult InterviewHistory(Guid id)
        {
            return this.View(interviewHistoryViewFactory.Load(id));
        }

        public ActionResult InterviewAreaFrame(Guid id, string questionId)
        {
            InterviewSummary interviewSummary = this.interviewSummaryViewFactory.Load(id);

            ChangeStatusView interviewInfo =
                this.changeStatusFactory.Load(new ChangeStatusInputModel { InterviewId = id });

            if (interviewInfo == null || interviewSummary == null || interviewSummary.IsDeleted)
                return HttpNotFound();

            bool isAccessAllowed =
                this.authorizedUser.IsHeadquarter || this.authorizedUser.IsAdministrator ||
                (this.authorizedUser.IsSupervisor && this.authorizedUser.Id == interviewSummary.TeamLeadId);

            if (!isAccessAllowed)
                return HttpNotFound();

            var detailsViewModel = interviewDetailsViewFactory.GetInterviewDetails(id, InterviewDetailsFilter.All, null);
            var identity = Identity.Parse(questionId);
            var question = detailsViewModel.FilteredEntities.FirstOrDefault(x => x.Id == identity) as InterviewQuestionView;

            if(question == null || question.QuestionType != QuestionType.Area)
                return HttpNotFound();

            return this.View(question);
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