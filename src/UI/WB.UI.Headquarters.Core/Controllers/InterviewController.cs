﻿using System;
using Humanizer;
using Main.Core.Documents;
using Main.Core.Entities.SubEntities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WB.Core.BoundedContexts.Headquarters;
using WB.Core.BoundedContexts.Headquarters.Services;
using WB.Core.BoundedContexts.Headquarters.Views;
using WB.Core.BoundedContexts.Headquarters.Views.Interview;
using WB.Core.BoundedContexts.Headquarters.Views.InterviewHistory;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Commands.Interview;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;
using WB.Core.SharedKernels.Questionnaire.Documents;
using WB.Core.SharedKernels.SurveyManagement.Web.Models;
using WB.UI.Headquarters.Filters;

namespace WB.Core.SharedKernels.SurveyManagement.Web.Controllers
{
    [Authorize(Roles = "Administrator, Headquarter, Supervisor")]
    public class InterviewController : Controller
    {
        private readonly IAuthorizedUser authorizedUser;
        private readonly IInterviewHistoryFactory interviewHistoryViewFactory;
        private readonly IAllInterviewsFactory interviewSummaryViewFactory;
        private readonly IStatefulInterviewRepository statefulInterviewRepository;
        private readonly IQuestionnaireStorage questionnaireRepository;

        public InterviewController(IAuthorizedUser authorizedUser,
            IAllInterviewsFactory interviewSummaryViewFactory,
            IInterviewHistoryFactory interviewHistoryViewFactory,
            IStatefulInterviewRepository statefulInterviewRepository, 
            IQuestionnaireStorage questionnaireRepository)
        {
            this.authorizedUser = authorizedUser;
            this.interviewSummaryViewFactory = interviewSummaryViewFactory;
            this.interviewHistoryViewFactory = interviewHistoryViewFactory;
            this.statefulInterviewRepository = statefulInterviewRepository;
            this.questionnaireRepository = questionnaireRepository;
        }

        private bool CurrentUserCanAccessInterview(InterviewSummary interviewSummary)
        {
            if (interviewSummary == null)
                return false;

            if (this.authorizedUser.IsHeadquarter || this.authorizedUser.IsAdministrator)
                return true;

            if (this.authorizedUser.IsSupervisor && this.authorizedUser.Id == interviewSummary.SupervisorId)
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
        [Route("Interview/Review/{id}")]
        [Route("Interview/Review/{id}/Cover")]
        public ActionResult Cover(Guid id)
        {
            var interview = this.statefulInterviewRepository.Get(id.FormatGuid());
            var questionnaire = this.questionnaireRepository.GetQuestionnaireDocument(interview.QuestionnaireIdentity);
            if (questionnaire.IsCoverPageSupported)
                return Redirect("Interview/Review/{id}/Section/" + questionnaire.CoverPageSectionId.FormatGuid());

            return Review(id, null);
        }        

        [ActivePage(MenuItem.Docs)]
        [Route("Interview/Review/{id}/Section/{url}")]
        public ActionResult Review(Guid id, string url)
        {
            InterviewSummary interviewSummary = this.interviewSummaryViewFactory.Load(id);
            bool isAccessAllowed = CurrentUserCanAccessInterview(interviewSummary);

            if (!isAccessAllowed)
                return NotFound();

            this.statefulInterviewRepository.Get(id.FormatGuid()); // put questionnaire to cache.
            var questionnaire = this.questionnaireRepository.GetQuestionnaireDocument(QuestionnaireIdentity.Parse(interviewSummary.QuestionnaireIdentity));

            ViewBag.SpecificPageCaption = interviewSummary.Key;
            ViewBag.ExcludeMarkupSpecific = true;

            return View("Review", new InterviewReviewModel(this.GetApproveReject(interviewSummary))
            {
                Id = id.FormatGuid(),
                CoverPageId = questionnaire.IsCoverPageSupported ? questionnaire.CoverPageSectionId.FormatGuid() : String.Empty,
                Key = interviewSummary.Key,
                LastUpdatedAtUtc = interviewSummary.UpdateDate,
                StatusName = interviewSummary.Status.ToLocalizeString(),
                Responsible = interviewSummary.ResponsibleName,
                Supervisor = interviewSummary.SupervisorName,
                AssignmentId = interviewSummary.AssignmentId,
                QuestionnaireTitle = interviewSummary.QuestionnaireTitle,
                QuestionnaireVersion = interviewSummary.QuestionnaireVersion,
                InterviewDuration = interviewSummary.InterviewDuration?.Humanize(),
                ResponsibleRole = interviewSummary.ResponsibleRole.ToString(),
                ResponsibleProfileUrl = interviewSummary.ResponsibleRole == UserRoles.Interviewer ?
                                            Url.Action("Profile", "Interviewer", new { id = interviewSummary.ResponsibleId }) :
                                            "javascript:void(0);",
                InterviewsUrl = Url.Action("Index", "Interviews")
            });
        }

        private ApproveRejectAllowed GetApproveReject(InterviewSummary interviewSummary)
        {
            ApproveRejectAllowed approveRejectAllowed = new ApproveRejectAllowed();
            approveRejectAllowed.InterviewersListUrl = 
                this.authorizedUser.IsAdministrator || this.authorizedUser.IsHeadquarter
                ? Url.Action("ResponsiblesCombobox", "Teams")
                : Url.Action("InterviewersCombobox", "Teams");

            if(!this.authorizedUser.IsObserving)
            {
                approveRejectAllowed.HqOrAdminUnapproveAllowed = interviewSummary.Status == InterviewStatus.ApprovedByHeadquarters &&
                                                                 (authorizedUser.IsHeadquarter || authorizedUser.IsAdministrator);
                approveRejectAllowed.HqOrAdminRejectAllowed = (interviewSummary.Status == InterviewStatus.Completed ||
                                                               interviewSummary.Status == InterviewStatus.ApprovedBySupervisor) &&
                                                              (authorizedUser.IsHeadquarter || authorizedUser.IsAdministrator);
                approveRejectAllowed.SupervisorRejectAllowed = (interviewSummary.Status == InterviewStatus.Completed ||
                                                                interviewSummary.Status == InterviewStatus.RejectedByHeadquarters) &&
                                                               authorizedUser.IsSupervisor;
                approveRejectAllowed.HqOrAdminApproveAllowed = (interviewSummary.Status == InterviewStatus.Completed ||
                                                                interviewSummary.Status == InterviewStatus.ApprovedBySupervisor) &&
                                                               (authorizedUser.IsHeadquarter || authorizedUser.IsAdministrator);
                approveRejectAllowed.SupervisorApproveAllowed = (interviewSummary.Status == InterviewStatus.Completed ||
                                                                 interviewSummary.Status == InterviewStatus.RejectedByHeadquarters) &&
                                                                authorizedUser.IsSupervisor;
            }

            approveRejectAllowed.InterviewerShouldbeSelected = approveRejectAllowed.SupervisorRejectAllowed
                                                               && !interviewSummary.IsAssignedToInterviewer; 

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
                return NotFound();

            bool isAccessAllowed = CurrentUserCanAccessInterview(interviewSummary);
            if (!isAccessAllowed)
                return NotFound();

            var identity = Identity.Parse(questionId);
            var interview = this.statefulInterviewRepository.Get(id.FormatGuid());
            var area = interview.GetAreaQuestion(identity)?.GetAnswer()?.Value;

            var questionnaire = this.questionnaireRepository.GetQuestionnaire(interview.QuestionnaireIdentity, interview.Language);
            var geometryType = questionnaire.GetQuestionGeometryType(identity.Id);

            return this.View(new GeographyPreview() { AreaAnswer = area, Geometry = geometryType ?? GeometryType.Polygon });
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
        
        public string CoverPageId { get; set; }

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
