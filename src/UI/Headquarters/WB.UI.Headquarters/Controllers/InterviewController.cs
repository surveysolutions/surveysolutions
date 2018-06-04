using System;
using System.Web.Mvc;
using Humanizer;
using Main.Core.Entities.SubEntities;
using WB.Core.BoundedContexts.Headquarters;
using WB.Core.BoundedContexts.Headquarters.Services;
using WB.Core.BoundedContexts.Headquarters.Views;
using WB.Core.BoundedContexts.Headquarters.Views.Interview;
using WB.Core.BoundedContexts.Headquarters.Views.InterviewHistory;
using WB.Core.BoundedContexts.Headquarters.WebInterview;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Commands.Interview;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;
using WB.Core.SharedKernels.Questionnaire.Documents;
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
        private readonly IInterviewHistoryFactory interviewHistoryViewFactory;
        private readonly IInterviewSummaryViewFactory interviewSummaryViewFactory;
        private readonly IStatefulInterviewRepository statefulInterviewRepository;
        private readonly IQuestionnaireStorage questionnaireRepository;
        private readonly IPauseResumeQueue pauseResumeQueue;

        public InterviewController(
            ICommandService commandService, 
            IAuthorizedUser authorizedUser,
            ILogger logger,
            IInterviewSummaryViewFactory interviewSummaryViewFactory,
            IInterviewHistoryFactory interviewHistoryViewFactory,
            IStatefulInterviewRepository statefulInterviewRepository,
            IPauseResumeQueue pauseResumeQueue, IQuestionnaireStorage questionnaireRepository)
            : base(commandService, logger)
        {
            this.authorizedUser = authorizedUser;
            this.interviewSummaryViewFactory = interviewSummaryViewFactory;
            this.interviewHistoryViewFactory = interviewHistoryViewFactory;
            this.statefulInterviewRepository = statefulInterviewRepository;
            this.pauseResumeQueue = pauseResumeQueue;
            this.questionnaireRepository = questionnaireRepository;
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
                this.pauseResumeQueue.EnqueueOpenBySupervisor(new OpenInterviewBySupervisorCommand(id, this.authorizedUser.Id));
            }

            ViewBag.SpecificPageCaption = interviewSummary.Key;
            ViewBag.AlwaysScroll = true;
            ViewBag.ExcludeMarkupSpecific = true;

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
            
            if (interviewSummary == null)
                return HttpNotFound();

            bool isAccessAllowed = CurrentUserCanAccessInterview(interviewSummary);
            if(!isAccessAllowed)
                return HttpNotFound();

            var identity = Identity.Parse(questionId);
            var interview = this.statefulInterviewRepository.Get(id.FormatGuid());
            var area = interview.GetAreaQuestion(identity)?.GetAnswer()?.Value;

            var questionnaire = this.questionnaireRepository.GetQuestionnaire(interview.QuestionnaireIdentity, interview.Language);
            var geometryType = questionnaire.GetQuestionByVariable(questionnaire.GetQuestionVariableName(identity.Id)).Properties
                .GeometryType;

            return this.View(new GeographyPreview(){AreaAnswer = area, Geometry = geometryType ?? GeometryType.Polygon});
        }
    }

    public class GeographyPreview
    {
        public Area AreaAnswer { set; get; }
        public GeometryType Geometry { set; get; }
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
