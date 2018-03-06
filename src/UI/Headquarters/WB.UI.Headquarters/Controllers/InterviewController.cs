using System;
using System.Linq;
using System.Web.Mvc;
using Humanizer;
using Main.Core.Entities.SubEntities;
using WB.Core.BoundedContexts.Headquarters;
using WB.Core.BoundedContexts.Headquarters.Services;
using WB.Core.BoundedContexts.Headquarters.Views;
using WB.Core.BoundedContexts.Headquarters.Views.ChangeStatus;
using WB.Core.BoundedContexts.Headquarters.Views.Interview;
using WB.Core.BoundedContexts.Headquarters.Views.InterviewHistory;
using WB.Core.BoundedContexts.Headquarters.WebInterview;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Commands.Interview;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;
using WB.Core.SharedKernels.SurveyManagement.Web.Filters;
using WB.Core.SharedKernels.SurveyManagement.Web.Models;
using WB.UI.Headquarters.Controllers;
using WB.UI.Headquarters.Filters;

namespace WB.Core.SharedKernels.SurveyManagement.Web.Controllers
{
    [LimitsFilter]
    [Authorize(Roles = "Administrator, Headquarter, Supervisor")]
    public class InterviewController : BaseController
    {
        private readonly IAuthorizedUser authorizedUser;
        private readonly IChangeStatusFactory changeStatusFactory;
        private readonly IInterviewHistoryFactory interviewHistoryViewFactory;
        private readonly IInterviewSummaryViewFactory interviewSummaryViewFactory;
        private readonly IInterviewDetailsViewFactory interviewDetailsViewFactory;
        private readonly IStatefulInterviewRepository statefulInterviewRepository;
        private readonly IPauseResumeQueue pauseResumeQueue;

        public InterviewController(
            ICommandService commandService, 
            IAuthorizedUser authorizedUser,
            ILogger logger,
            IChangeStatusFactory changeStatusFactory,
            IInterviewSummaryViewFactory interviewSummaryViewFactory,
            IInterviewHistoryFactory interviewHistoryViewFactory, 
            IInterviewDetailsViewFactory interviewDetailsViewFactory,
            IStatefulInterviewRepository statefulInterviewRepository,
            IPauseResumeQueue pauseResumeQueue)
            : base(commandService, logger)
        {
            this.authorizedUser = authorizedUser;
            this.changeStatusFactory = changeStatusFactory;
            this.interviewSummaryViewFactory = interviewSummaryViewFactory;
            this.interviewHistoryViewFactory = interviewHistoryViewFactory;
            this.interviewDetailsViewFactory = interviewDetailsViewFactory;
            this.statefulInterviewRepository = statefulInterviewRepository;
            this.pauseResumeQueue = pauseResumeQueue;
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

            if (interviewInfo == null || interviewSummary == null)
                return HttpNotFound();

            bool isAccessAllowed = CurrentUserCanAccessInterview(interviewSummary);

            if (!isAccessAllowed)
                return HttpNotFound();

            var detailsViewModel = interviewDetailsViewFactory.GetInterviewDetails(interviewId: id,
                questionsTypes: questionsTypes ?? InterviewDetailsFilter.All,
                currentGroupIdentity: string.IsNullOrEmpty(currentGroupId) ? null : Identity.Parse(currentGroupId));

            detailsViewModel.ApproveReject = GetApproveReject(interviewSummary);

            return View(detailsViewModel);
        }

        private bool CurrentUserCanAccessInterview(InterviewSummary interviewSummary)
        {
            if (interviewSummary == null)
                return false;

            if (this.authorizedUser.IsHeadquarter || this.authorizedUser.IsAdministrator)
                return true;

            if (this.authorizedUser.IsSupervisor && this.authorizedUser.Id == interviewSummary.TeamLeadId)
            {
                var hasSupervisorAccessToInterview = interviewSummary.Status == InterviewStatus.InterviewerAssigned
                                                    || interviewSummary.Status == InterviewStatus.SupervisorAssigned
                                                    || interviewSummary.Status == InterviewStatus.Completed
                                                    || interviewSummary.Status == InterviewStatus.RejectedBySupervisor
                                                    || interviewSummary.Status == InterviewStatus.RejectedByHeadquarters;

                if (hasSupervisorAccessToInterview)
                    return true;

            }

            return false;
        }

        [ActivePage(MenuItem.Docs)]
        public ActionResult Review(Guid id, string url)
        {
            InterviewSummary interviewSummary = this.interviewSummaryViewFactory.Load(id);
            bool isAccessAllowed = CurrentUserCanAccessInterview(interviewSummary);

            if (!isAccessAllowed)
                return HttpNotFound();

            this.statefulInterviewRepository.Get(id.FormatGuid()); // put questionnaire to cache.


            if (this.authorizedUser.IsSupervisor)
            {
                this.pauseResumeQueue.EnqueueOpenBySupervisor(new OpenInterviewBySupervisorCommand(id, this.authorizedUser.Id, DateTime.Now));
            }

            ViewBag.SpecificPageCaption = interviewSummary.Key;
            ViewBag.AlwaysScroll = true;

            return View(new InterviewReviewModel(this.GetApproveReject(interviewSummary))
            {
                Id = id.FormatGuid(),
                Key = interviewSummary.Key,
                LastUpdatedAtUtc = interviewSummary.UpdateDate,
                StatusName = interviewSummary.Status.ToLocalizeString(),
                Responsible = interviewSummary.ResponsibleName,
                Supervisor = interviewSummary.TeamLeadName,
                AssignmentId = interviewSummary.AssignmentId,
                QuestionnaireTitle = interviewSummary.QuestionnaireTitle,
                QuestionnaireVersion = interviewSummary.QuestionnaireVersion,
                InterviewDuration = interviewSummary.InterviewDuration?.Humanize(),
                ResponsibleRole = interviewSummary.ResponsibleRole.ToString(),
                ResponsibleProfileUrl = interviewSummary.ResponsibleRole == UserRoles.Interviewer ?
                                            Url.Action("Profile", "Interviewer", new {id = interviewSummary.ResponsibleId}) : 
                                            "javascript:void(0);",
                InterviewsUrl = authorizedUser.IsSupervisor ? Url.Action("Interviews", "Survey") : Url.Action("Interviews", "HQ")
            });
        }

        private ApproveRejectAllowed GetApproveReject(InterviewSummary interviewSummary)
        {
            var approveRejectAllowed = new ApproveRejectAllowed
            {
                SupervisorApproveAllowed = (interviewSummary.Status == InterviewStatus.Completed || interviewSummary.Status == InterviewStatus.RejectedByHeadquarters) &&
                                           authorizedUser.IsSupervisor,
                HqOrAdminApproveAllowed = (interviewSummary.Status == InterviewStatus.Completed || interviewSummary.Status == InterviewStatus.ApprovedBySupervisor) &&
                                          (authorizedUser.IsHeadquarter || authorizedUser.IsAdministrator),
                SupervisorRejectAllowed = (interviewSummary.Status == InterviewStatus.Completed || interviewSummary.Status == InterviewStatus.RejectedByHeadquarters) &&
                                          authorizedUser.IsSupervisor,
                HqOrAdminRejectAllowed = (interviewSummary.Status == InterviewStatus.Completed || interviewSummary.Status == InterviewStatus.ApprovedBySupervisor) &&
                                         (authorizedUser.IsHeadquarter || authorizedUser.IsAdministrator),
                HqOrAdminUnapproveAllowed = interviewSummary.Status == InterviewStatus.ApprovedByHeadquarters && (authorizedUser.IsHeadquarter || authorizedUser.IsAdministrator),
                InterviewersListUrl = Url.RouteUrl("DefaultApiWithAction", new { httproute = "", controller = "Teams", action = "InterviewersCombobox" })
        };
            approveRejectAllowed.InterviewerShouldbeSelected = approveRejectAllowed.SupervisorRejectAllowed && !interviewSummary.IsAssignedToInterviewer;
            return approveRejectAllowed;
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

            if (interviewInfo == null || interviewSummary == null)
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
    }

    public class InterviewReviewModel
    {
        public InterviewReviewModel(ApproveRejectAllowed approveRejectAllowed)
        {
            this.ApproveReject = approveRejectAllowed;
        }

        public string Id { get; set; }
        
        public string Key { get; set; }

        public DateTime LastUpdatedAtUtc { get; set; }

        public ApproveRejectAllowed ApproveReject { get; }

        public string StatusName { get; set; }

        public string Responsible { get; set; }

        public string ResponsibleRole { get; set; }

        public string InterviewsUrl { get; set; }

        public string ResponsibleProfileUrl { get; set; }
        public string Supervisor { get; set; }
        public int? AssignmentId { get; set; }
        public string QuestionnaireTitle { get; set; }
        public long QuestionnaireVersion { get; set; }
        public string InterviewDuration { get; set; }
    }
}