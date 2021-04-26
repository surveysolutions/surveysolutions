using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Humanizer;
using MvvmCross.Commands;
using NodaTime;
using NodaTime.Extensions;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.GenericSubdomains.Portable.ServiceLocation;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.SharedKernels.DataCollection.Commands.CalendarEvent;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;
using WB.Core.SharedKernels.DataCollection.Views.InterviewerAuditLog.Entities;
using WB.Core.SharedKernels.Enumerator.Properties;
using WB.Core.SharedKernels.Enumerator.Repositories;
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

        private ICommandService CommandService =>
            serviceLocator.GetInstance<ICommandService>();

        private IEnumeratorSettings Settings => serviceLocator.GetInstance<IEnumeratorSettings>();

        private IPrincipal Principal => serviceLocator.GetInstance<IPrincipal>();

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
                ActionType = ActionType.Context,
                Command = new MvxAsyncCommand(this.SetCalendarEventAsync, 
                    () => this.isInterviewReadyToLoad && interview.Status != InterviewStatus.Completed),
                Label = interview.CalendarEvent.HasValue 
                    ? EnumeratorUIResources.Dashboard_EditCalendarEvent
                    : EnumeratorUIResources.Dashboard_AddCalendarEvent
            });

            Actions.Add(new ActionDefinition
            {
                ActionType = ActionType.Context,
                Command = new MvxCommand(this.RemoveCalendarEvent, 
                    () => this.isInterviewReadyToLoad && interview.CalendarEvent.HasValue 
                                                      && interview.Status != InterviewStatus.Completed),
                Label = EnumeratorUIResources.Dashboard_RemoveCalendarEvent
            });

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

            if(interview.Mode == InterviewMode.CAWI)
            {
                Actions.Add(new ActionDefinition
                {
                    ActionType = ActionType.Secondary,
                    Command = new MvxAsyncCommand(this.ShowQRCodeAsync),
                    Label = EnumeratorUIResources.DashboardItem_QRCode
                });
            }

            string MainLabel()
            {
                if (interview.Mode == InterviewMode.CAWI)
                    return EnumeratorUIResources.Dashboard_Reopen;

                return Status switch
                {
                    DashboardInterviewStatus.New => EnumeratorUIResources.Dashboard_Open,
                    DashboardInterviewStatus.InProgress => EnumeratorUIResources.Dashboard_Open,
                    DashboardInterviewStatus.Completed => EnumeratorUIResources.Dashboard_Reopen,
                    DashboardInterviewStatus.Rejected => EnumeratorUIResources.Dashboard_ViewIssues,
                    _ => throw new ArgumentOutOfRangeException()
                };
            }

            string DiscardLabel()
            {
                return Status switch
                {
                    DashboardInterviewStatus.Completed => EnumeratorUIResources.Dashboard_Discard,
                    DashboardInterviewStatus.New => EnumeratorUIResources.Dashboard_Discard,
                    DashboardInterviewStatus.InProgress => EnumeratorUIResources.Dashboard_Discard,
                    DashboardInterviewStatus.Rejected => EnumeratorUIResources.Dashboard_Discard,
                    _ => throw new ArgumentOutOfRangeException()
                };
            }
        }

        private async Task ShowQRCodeAsync()
        {
            //interview created before assignments
            if (interview.Assignment == null)
                return;

            var qrCodeText = Settings.RenderWebInterviewUri(interview.Assignment.Value, interview.InterviewId);
            await UserInteractionService.ShowAlertWithQRCodeAndText(qrCodeText, interview.InterviewKey);
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
                this.IdLabel = EnumeratorUIResources.Dashboard_No_InterviewKey;

            Title = string.Format(EnumeratorUIResources.DashboardItem_Title, interview.QuestionnaireTitle, questionnaireIdentity.Version);
            
            this.CalendarEventStart = GetCalendarEventDate();
            this.CalendarEventComment = interview.CalendarEventComment;

            this.AssignmentIdLabel = interview.Assignment.HasValue
                ? EnumeratorUIResources.Dashboard_Interview_AssignmentLabelFormat.FormatString(interview.Assignment)
                : EnumeratorUIResources.Dashboard_CensusAssignment;
        }

        private string GetSubTitle()
        {
            var comment = GetInterviewCommentByStatus(interview);
            var dateComment = GetInterviewDateCommentByStatus(interview);

            string separator = !string.IsNullOrEmpty(comment) ? Environment.NewLine : string.Empty;
            var subTitle = $"{dateComment}{separator}{comment}";
            return subTitle;
        }

        public override string SubTitle => GetSubTitle();

        private ZonedDateTime? GetCalendarEventDate()
        {
            if (interview.CalendarEvent.HasValue)
            {
                return GetZonedDateTime(interview.CalendarEvent.Value, interview.CalendarEventTimezoneId);
            }

            return null;
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
                    return FormatDateTimeString(EnumeratorUIResources.DashboardItem_AssignedOn, interview.InterviewerAssignedDateTime);
                case DashboardInterviewStatus.New:
                    return FormatDateTimeString(EnumeratorUIResources.DashboardItem_AssignedOn, interview.InterviewerAssignedDateTime);
                case DashboardInterviewStatus.InProgress:
                    return FormatDateTimeString(EnumeratorUIResources.DashboardItem_StartedOn, interview.StartedDateTime);
                case DashboardInterviewStatus.Completed:
                    return FormatDateTimeString(EnumeratorUIResources.DashboardItem_CompletedOn, interview.CompletedDateTime);
                case DashboardInterviewStatus.Rejected:
                    return FormatDateTimeString(EnumeratorUIResources.DashboardItem_RejectedOn, interview.RejectedDateTime);
                case DashboardInterviewStatus.RejectedByHeadquarters:
                    return FormatDateTimeString(EnumeratorUIResources.DashboardItem_RejectedByHqOn, interview.RejectedDateTime);
                case DashboardInterviewStatus.ApprovedBySupervisor:
                    return FormatDateTimeString(EnumeratorUIResources.DashboardItem_ApprovedBySupervisor, interview.ApprovedDateTimeUtc);
                default:
                    return string.Empty;
            }
        }

        private DashboardInterviewStatus GetDashboardCategoryForInterview(
            InterviewStatus interviewStatus, DateTime? startedDateTime)
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
                case DashboardInterviewStatus.ApprovedBySupervisor:
                case DashboardInterviewStatus.RejectedByHeadquarters:
                    return interview.LastInterviewerOrSupervisorComment;
                default:
                    return string.Empty;
            }
        }
        
        private async Task RemoveInterviewAsync()
        {
            this.isInterviewReadyToLoad = false;

            var isNeedDelete = await this.UserInteractionService.ConfirmAsync(
                EnumeratorUIResources.Dashboard_RemoveInterviewQuestion.FormatString(this.interview.InterviewKey),
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
                if (this.Status == DashboardInterviewStatus.Completed || interview.Mode == InterviewMode.CAWI)
                {
                    var isReopen = await this.UserInteractionService.ConfirmAsync(
                        EnumeratorUIResources.Dashboard_Reinitialize_Interview_Message,
                        okButton: UIResources.Yes,
                        cancelButton: UIResources.No);

                    if (!isReopen)
                    {
                        return;
                    }
                }

                Logger.Warn($"Open Interview {this.interview.InterviewId} (key: {this.interview.InterviewKey}, assignment: {this.interview.Assignment}) at {DateTime.Now}");
                await this.ViewModelNavigationService.NavigateToAsync<LoadingInterviewViewModel, LoadingViewModelArg>(new LoadingViewModelArg
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

        private async Task SetCalendarEventAsync()
        {
            if (interview.Assignment.HasValue)
            {
                await ViewModelNavigationService.NavigateToAsync<CalendarEventDialogViewModel, CalendarEventViewModelArgs>(
                    new CalendarEventViewModelArgs(
                        interview.InterviewId,
                        interview.InterviewKey,
                        interview.Assignment.Value,
                        RaiseOnItemUpdated)
                );
            }
        }

        private void RemoveCalendarEvent()
        {
            var calendarEventStorage = serviceLocator.GetInstance<ICalendarEventStorage>();
            var calendarEvent = calendarEventStorage.GetCalendarEventForInterview(interview.InterviewId);

            if (interview.Assignment.HasValue && calendarEvent != null)
            {
                var command = new DeleteCalendarEventCommand(calendarEvent.Id,
                    Principal.CurrentUserIdentity.UserId);
                CommandService.Execute(command);
                
                RaiseOnItemUpdated();    
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
