﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using MvvmCross.Core.ViewModels;
using WB.Core.BoundedContexts.Interviewer.Properties;
using WB.Core.BoundedContexts.Interviewer.Services.Infrastructure;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.Enumerator.Properties;
using WB.Core.SharedKernels.Enumerator.Services;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure.Storage;

namespace WB.Core.BoundedContexts.Interviewer.Views.Dashboard.DashboardItems
{
    public class InterviewDashboardItemViewModel : IDashboardItem
    {
        private readonly IViewModelNavigationService viewModelNavigationService;
        private readonly IUserInteractionService userInteractionService;
        private readonly IExternalAppLauncher externalAppLauncher;
        private readonly IPlainStorage<QuestionnaireView> questionnaireViewRepository;
        private readonly IPlainStorage<PrefilledQuestionView> prefilledQuestions;
        private readonly IInterviewerInterviewAccessor interviewerInterviewFactory;

        public string QuestionnaireName { get; private set; }
        public Guid InterviewId { get; private set; }
        public DashboardInterviewStatus Status { get; private set; }
        public List<PrefilledQuestion> PrefilledQuestions { get; private set; }
        public List<PrefilledQuestion> DetailedPrefilledQuestions { get; private set; }
        public string DateComment { get; private set; }
        public string Comment { get; private set; }

        public event EventHandler OnItemRemoved;

        public InterviewDashboardItemViewModel(
            IViewModelNavigationService viewModelNavigationService,
            IUserInteractionService userInteractionService,
            IExternalAppLauncher externalAppLauncher,
            IPlainStorage<QuestionnaireView> questionnaireViewRepository,
            IPlainStorage<PrefilledQuestionView> prefilledQuestions,
            IInterviewerInterviewAccessor interviewFactory)
        {
            this.viewModelNavigationService = viewModelNavigationService;
            this.userInteractionService = userInteractionService;
            this.externalAppLauncher = externalAppLauncher;
            this.questionnaireViewRepository = questionnaireViewRepository;
            this.prefilledQuestions = prefilledQuestions;
            this.interviewerInterviewFactory = interviewFactory;
        }

        public void Init(InterviewView interview)
        {
            var questionnaire = this.questionnaireViewRepository.GetById(interview.QuestionnaireId);

            var questionnaireIdentity = QuestionnaireIdentity.Parse(questionnaire.Id);

            this.InterviewId = interview.InterviewId;
            this.Status = this.GetDashboardCategoryForInterview(interview.Status, interview.StartedDateTime);
            this.QuestionnaireName = string.Format(InterviewerUIResources.DashboardItem_Title, questionnaire.Title, questionnaireIdentity.Version);
            this.DateComment = this.GetInterviewDateCommentByStatus(interview);
            this.Comment = this.GetInterviewCommentByStatus(interview);
            var questions = this.GetPrefilledQuestions();

            this.PrefilledQuestions = questions.Take(3).ToList();
            this.DetailedPrefilledQuestions = questions.Skip(3).ToList();

            this.GpsLocation = this.GetInterviewLocation(interview);
            this.IsSupportedRemove = interview.CanBeDeleted;
            this.HasExpandedView = this.PrefilledQuestions.Count > 0;

            if (interview.Assignment != null)
            {
                this.Title = string.Format(InterviewerUIResources.Dashboard_InterviewCard_Title, interview.Assignment);
            }
        }

        public string Title { get; private set; }

        private InterviewGpsCoordinatesView GetInterviewLocation(InterviewView interview)
        {
            if (interview.LocationQuestionId.HasValue && interview.LocationLatitude.HasValue && interview.LocationLongitude.HasValue)
            {
                return new InterviewGpsCoordinatesView
                {
                    Latitude = interview.LocationLatitude ?? 0,
                    Longitude = interview.LocationLongitude ?? 0
                };
            }

            return null;
        }

        private DashboardInterviewStatus GetDashboardCategoryForInterview(InterviewStatus interviewStatus, DateTime? startedDateTime)
        {
            switch (interviewStatus)
            {
                case InterviewStatus.RejectedBySupervisor:
                    return DashboardInterviewStatus.Rejected;
                case InterviewStatus.Completed:
                    return DashboardInterviewStatus.Completed;
                case InterviewStatus.Restarted:
                    return DashboardInterviewStatus.InProgress;
                case InterviewStatus.InterviewerAssigned:
                    return startedDateTime.HasValue
                        ? DashboardInterviewStatus.InProgress
                        : DashboardInterviewStatus.New;

                default:
                    throw new ArgumentException("Can't identify status for interview: {0}".FormatString(interviewStatus));
            }
        }

        public InterviewGpsCoordinatesView GpsLocation { get; private set; }
        public bool HasGpsLocation => this.GpsLocation != null;

        public IMvxCommand NavigateToGpsLocationCommand
        {
            get { return new MvxCommand(this.NavigateToGpsLocation, () => this.HasGpsLocation); }
        }

        private void NavigateToGpsLocation()
        {
            this.externalAppLauncher.LaunchMapsWithTargetLocation(this.GpsLocation.Latitude, this.GpsLocation.Longitude);
        }

        private string GetInterviewDateCommentByStatus(InterviewView interview)
        {
            switch (this.Status)
            {
                case DashboardInterviewStatus.New:
                    return FormatDateTimeString(InterviewerUIResources.DashboardItem_AssignedOn, interview.InterviewerAssignedDateTime);
                case DashboardInterviewStatus.InProgress:
                    return FormatDateTimeString(InterviewerUIResources.DashboardItem_StartedOn, interview.StartedDateTime);
                case DashboardInterviewStatus.Completed:
                    return FormatDateTimeString(InterviewerUIResources.DashboardItem_CompletedOn, interview.CompletedDateTime);
                case DashboardInterviewStatus.Rejected:
                    return FormatDateTimeString(InterviewerUIResources.DashboardItem_RejectedOn, interview.RejectedDateTime);
                default:
                    return string.Empty;
            }
        }

        private string FormatDateTimeString(string formatString, DateTime? utcDateTimeWithOutKind)
        {
            if (!utcDateTimeWithOutKind.HasValue)
                return string.Empty;

            var utcDateTime = DateTime.SpecifyKind(utcDateTimeWithOutKind.Value, DateTimeKind.Utc);
            return string.Format(formatString, utcDateTime.ToLocalTime().ToString(CultureInfo.CurrentUICulture));
        }

        private string GetInterviewCommentByStatus(InterviewView interview)
        {
            switch (this.Status)
            {
                case DashboardInterviewStatus.New:
                    return InterviewerUIResources.DashboardItem_NotStarted;
                case DashboardInterviewStatus.Completed:
                    return interview.LastInterviewerOrSupervisorComment;
                case DashboardInterviewStatus.Rejected:
                    return interview.LastInterviewerOrSupervisorComment;
                default:
                    return string.Empty;
            }
        }

        private List<PrefilledQuestion> GetPrefilledQuestions()
        {
            return this.prefilledQuestions.Where(_ => _.InterviewId == this.InterviewId)
                                          .OrderBy(x => x.SortIndex)
                                          .Select(fi => new PrefilledQuestion {
                                              Answer = fi.Answer,
                                              Question = fi.QuestionText
                                          }).ToList();
        }

        public bool IsSupportedRemove { get; set; }

        public IMvxAsyncCommand RemoveInterviewCommand
            => new MvxAsyncCommand(this.RemoveInterviewAsync, () => this.isInterviewReadyToLoad);

        private async Task RemoveInterviewAsync()
        {
            this.isInterviewReadyToLoad = false;

            var isNeedDelete = await this.userInteractionService.ConfirmAsync(
                InterviewerUIResources.Dashboard_RemoveInterviewQuestion.FormatString(this.QuestionnaireName),
                okButton: UIResources.Yes,
                cancelButton: UIResources.No);

            if (!isNeedDelete)
            {
                this.isInterviewReadyToLoad = true;
                return;
            }
            this.interviewerInterviewFactory.RemoveInterview(this.InterviewId);
            this.OnItemRemoved(this, EventArgs.Empty);
        }

        public IMvxAsyncCommand LoadDashboardItemCommand
            => new MvxAsyncCommand(this.LoadInterviewAsync, () => this.isInterviewReadyToLoad);

        public bool HasExpandedView { get; private set; }

        private bool isInterviewReadyToLoad = true;

        public async Task LoadInterviewAsync()
        {
            this.isInterviewReadyToLoad = false;
            try
            {
                if (this.Status == DashboardInterviewStatus.Completed)
                {
                    var isReopen = await this.userInteractionService.ConfirmAsync(
                        InterviewerUIResources.Dashboard_Reinitialize_Interview_Message,
                        okButton: UIResources.Yes,
                        cancelButton: UIResources.No);

                    if (!isReopen)
                    {
                        return;
                    }
                    
                }

                this.viewModelNavigationService.NavigateTo<LoadingViewModel>(new {interviewId = this.InterviewId});
            }
            finally
            {
                this.isInterviewReadyToLoad = true;
            }
        }
    }
}