using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using MvvmCross.Commands;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.GenericSubdomains.Portable.ServiceLocation;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;
using WB.Core.SharedKernels.DataCollection.Views.InterviewerAuditLog.Entities;
using WB.Core.SharedKernels.Enumerator.Properties;
using WB.Core.SharedKernels.Enumerator.Services;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure.Storage;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewLoading;
using WB.Core.SharedKernels.Enumerator.Views;
using WB.Core.SharedKernels.Enumerator.Views.Dashboard;

namespace WB.Core.SharedKernels.Enumerator.ViewModels.Dashboard
{
    public class InterviewDashboardItemViewModel : ExpandableQuestionsDashboardItemViewModel, IDashboardViewItem
    {
        private readonly IServiceLocator serviceLocator;
        private readonly IAuditLogService auditLogService;

        protected IViewModelNavigationService ViewModelNavigationService =>
            serviceLocator.GetInstance<IViewModelNavigationService>();

        private IUserInteractionService UserInteractionService =>
            serviceLocator.GetInstance<IUserInteractionService>();

        private IPlainStorage<QuestionnaireView> QuestionnaireViewRepository =>
            serviceLocator.GetInstance<IPlainStorage<QuestionnaireView>>();
        
        private IInterviewerInterviewAccessor InterviewerInterviewFactory =>
            serviceLocator.GetInstance<IInterviewerInterviewAccessor>();

        protected ILogger Logger => serviceLocator.GetInstance<ILoggerProvider>().GetForType(typeof(InterviewDashboardItemViewModel));

        public event EventHandler OnItemRemoved;
        protected bool isInterviewReadyToLoad = true;
        private QuestionnaireIdentity questionnaireIdentity;
        protected InterviewView interview;
        private string assignmentIdLabel;

        public InterviewDashboardItemViewModel(IServiceLocator serviceLocator, IAuditLogService auditLogService) : base(serviceLocator)
        {
            this.serviceLocator = serviceLocator;
            this.auditLogService = auditLogService;
        }

        public void Init(InterviewView interviewView, List<PrefilledQuestion> details)
        {
            this.interview = interviewView;
            this.questionnaireIdentity = QuestionnaireIdentity.Parse(interview.QuestionnaireId);
            this.Status = this.GetDashboardCategoryForInterview(interview.Status, interview.StartedDateTime);

            BindDetails(details);
            BindTitles();
            BindActions();
            this.RaiseAllPropertiesChanged();
        }

        public Guid InterviewId => this.interview.InterviewId;

        protected virtual void BindActions()
        {
            Actions.Clear();

            BindLocationAction(interview.LocationQuestionId, interview.LocationLatitude, interview.LocationLongitude);

            Actions.Add(new ActionDefinition
            {
                ActionType = ActionType.Primary,
                Command = new MvxAsyncCommand(this.LoadInterviewAsync, () => this.isInterviewReadyToLoad),
                Label = MainLabel()
            });

            Actions.Add(new ActionDefinition
            {
                ActionType = ActionType.Context,
                Command = new MvxAsyncCommand(this.RemoveInterviewAsync, () => this.isInterviewReadyToLoad && interview.CanBeDeleted),
                Label = DiscardLabel()
            });

            string MainLabel()
            {
                switch (Status) {
                    case DashboardInterviewStatus.New:
                        return InterviewerUIResources.Dashboard_Open;
                    case DashboardInterviewStatus.InProgress:
                        return InterviewerUIResources.Dashboard_Open;
                    case DashboardInterviewStatus.Completed:
                        return InterviewerUIResources.Dashboard_Reopen;
                    case DashboardInterviewStatus.Rejected:
                        return InterviewerUIResources.Dashboard_ViewIssues;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }

            string DiscardLabel()
            {
                switch (Status)
                {
                    case DashboardInterviewStatus.Completed:
                    case DashboardInterviewStatus.New:
                    case DashboardInterviewStatus.InProgress:
                    case DashboardInterviewStatus.Rejected:
                        return InterviewerUIResources.Dashboard_Discard;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }

        private void BindDetails(List<PrefilledQuestion> preffilledQuestions)
        {
            this.DetailedIdentifyingData = preffilledQuestions;
            this.IdentifyingData = this.DetailedIdentifyingData.Take(3).ToList();
            this.HasExpandedView = this.PrefilledQuestions.Count > 0;
            this.IsExpanded = false;
        }

        protected virtual void BindTitles()
        {
            if (string.IsNullOrWhiteSpace(interview.QuestionnaireTitle)) // only to support existing clients
            {
                var questionnaire = this.QuestionnaireViewRepository.GetById(interview.QuestionnaireId);
                interview.QuestionnaireTitle = questionnaire.Title;
            }

            if (!string.IsNullOrWhiteSpace(interview.InterviewKey))
                this.IdLabel = interview.InterviewKey;
            else
                this.IdLabel = InterviewerUIResources.Dashboard_No_InterviewKey;

            Title = string.Format(InterviewerUIResources.DashboardItem_Title, interview.QuestionnaireTitle, questionnaireIdentity.Version);
            
            var comment = GetInterviewCommentByStatus(interview);
            var dateComment = GetInterviewDateCommentByStatus(interview);

            string separator = !string.IsNullOrEmpty(comment) ? Environment.NewLine : string.Empty;
            this.SubTitle = $"{dateComment}{separator}{comment}";
            
            this.AssignmentIdLabel = interview.Assignment.HasValue
                ? InterviewerUIResources.Dashboard_Interview_AssignmentLabelFormat.FormatString(interview.Assignment)
                : InterviewerUIResources.Dashboard_CensusAssignment;
        }

        public string AssignmentIdLabel
        {
            get => assignmentIdLabel;
            set => SetProperty(ref this.assignmentIdLabel, value);
        }

        public int? AssignmentId => this.interview.Assignment;
        
        private string GetInterviewDateCommentByStatus(InterviewView interview)
        {
            switch (this.Status)
            {
                case DashboardInterviewStatus.Assignment:
                    return FormatDateTimeString(InterviewerUIResources.DashboardItem_AssignedOn, interview.InterviewerAssignedDateTime);
                case DashboardInterviewStatus.New:
                    return FormatDateTimeString(InterviewerUIResources.DashboardItem_AssignedOn, interview.InterviewerAssignedDateTime);
                case DashboardInterviewStatus.InProgress:
                    return FormatDateTimeString(InterviewerUIResources.DashboardItem_StartedOn, interview.StartedDateTime);
                case DashboardInterviewStatus.Completed:
                    return FormatDateTimeString(InterviewerUIResources.DashboardItem_CompletedOn, interview.CompletedDateTime);
                case DashboardInterviewStatus.Rejected:
                    return FormatDateTimeString(InterviewerUIResources.DashboardItem_RejectedOn, interview.RejectedDateTime);
                case DashboardInterviewStatus.RejectedByHeadquarters:
                    return FormatDateTimeString(InterviewerUIResources.DashboardItem_RejectedByHqOn, interview.RejectedDateTime);
                default:
                    return string.Empty;
            }
        }

        private string FormatDateTimeString(string formatString, DateTime? utcDateTimeWithOutKind)
        {
            if (!utcDateTimeWithOutKind.HasValue)
                return string.Empty;
            
            var utcDateTime = DateTime.SpecifyKind(utcDateTimeWithOutKind.Value, DateTimeKind.Utc);
            var culture = CultureInfo.CurrentUICulture;
            return string.Format(formatString, utcDateTime.ToLocalTime().ToString("MMM dd, HH:mm", culture).ToPascalCase());
        }

        private DashboardInterviewStatus GetDashboardCategoryForInterview(InterviewStatus interviewStatus, DateTime? startedDateTime)
        {
            switch (interviewStatus)
            {
                case InterviewStatus.RejectedByHeadquarters:
                    return DashboardInterviewStatus.RejectedByHeadquarters;
                case InterviewStatus.RejectedBySupervisor:
                    return DashboardInterviewStatus.Rejected;
                case InterviewStatus.ApprovedBySupervisor:
                    return DashboardInterviewStatus.ApprovedBySupervisor;
                case InterviewStatus.Restarted:
                    return DashboardInterviewStatus.InProgress;
                case InterviewStatus.Completed:
                    return DashboardInterviewStatus.Completed;
                case InterviewStatus.InterviewerAssigned:
                    return startedDateTime.HasValue
                        ? DashboardInterviewStatus.InProgress
                        : DashboardInterviewStatus.New;
                default:
                    throw new ArgumentException("Can't identify status for interview: {0}".FormatString(interviewStatus));
            }
        }

        private string GetInterviewCommentByStatus(InterviewView interview)
        {
            switch (this.Status)
            {
                case DashboardInterviewStatus.New:
                    return string.Empty;
                case DashboardInterviewStatus.Completed:
                case DashboardInterviewStatus.Rejected:
                    return interview.LastInterviewerOrSupervisorComment;
                default:
                    return string.Empty;
            }
        }
        
        private async Task RemoveInterviewAsync()
        {
            this.isInterviewReadyToLoad = false;

            var isNeedDelete = await this.UserInteractionService.ConfirmAsync(
                InterviewerUIResources.Dashboard_RemoveInterviewQuestion.FormatString(this.interview.InterviewKey),
                okButton: UIResources.Yes,
                cancelButton: UIResources.No);

            if (!isNeedDelete)
            {
                this.isInterviewReadyToLoad = true;
                return;
            }

            Logger.Warn($"Remove Interview {this.interview.InterviewId} (key: {this.interview.InterviewKey}, assignment: {this.interview.Assignment}) at {DateTime.Now}");
            this.InterviewerInterviewFactory.RemoveInterview(this.interview.InterviewId);
            auditLogService.Write(new DeleteInterviewAuditLogEntity(this.interview.InterviewId, this.interview.InterviewKey, this.interview.Assignment));
            this.OnItemRemoved.Invoke(this, EventArgs.Empty);
        }

        public virtual async Task LoadInterviewAsync()
        {
            this.isInterviewReadyToLoad = false;
            try
            {
                if (this.Status == DashboardInterviewStatus.Completed)
                {
                    var isReopen = await this.UserInteractionService.ConfirmAsync(
                        InterviewerUIResources.Dashboard_Reinitialize_Interview_Message,
                        okButton: UIResources.Yes,
                        cancelButton: UIResources.No);

                    if (!isReopen)
                    {
                        return;
                    }
                }

                Logger.Warn($"Open Interview {this.interview.InterviewId} (key: {this.interview.InterviewKey}, assignment: {this.interview.Assignment}) at {DateTime.Now}");
                await this.ViewModelNavigationService.NavigateToAsync<LoadingViewModel, LoadingViewModelArg>(new LoadingViewModelArg
                {
                    InterviewId = this.interview.InterviewId,
                    ShouldReopen = true
                });
            }
            finally
            {
                this.isInterviewReadyToLoad = true;
            }
        }

        
        private string responsible;
        public string Responsible
        {
            get => responsible;
            set => this.SetProperty(ref this.responsible, value);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
            }

            base.Dispose(disposing);
        }
    }
}
