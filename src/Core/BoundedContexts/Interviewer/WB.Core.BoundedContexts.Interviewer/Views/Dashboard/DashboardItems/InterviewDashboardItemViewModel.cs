using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Cirrious.MvvmCross.ViewModels;
using MvvmCross.Plugins.Messenger;
using WB.Core.BoundedContexts.Interviewer.ChangeLog;
using WB.Core.BoundedContexts.Interviewer.Properties;
using WB.Core.BoundedContexts.Interviewer.Services;
using WB.Core.BoundedContexts.Interviewer.Views.Dashboard.Messages;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.SharedKernels.DataCollection.Commands.Interview;
using WB.Core.SharedKernels.Enumerator.Aggregates;
using WB.Core.SharedKernels.Enumerator.Properties;
using WB.Core.SharedKernels.Enumerator.Repositories;
using WB.Core.SharedKernels.Enumerator.Services;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure;
using WB.Core.BoundedContexts.Interviewer.ViewModel.Dashboard;
using WB.UI.Interviewer.ViewModel.Dashboard;

namespace WB.Core.BoundedContexts.Interviewer.Views.Dashboard.DashboardItems
{
    public class InterviewDashboardItemViewModel : IDashboardItem
    {
        private readonly IViewModelNavigationService viewModelNavigationService;
        private readonly IUserInteractionService userInteractionService;
        private readonly IStatefulInterviewRepository interviewRepository;
        private readonly ICommandService commandService;
        private readonly IPrincipal principal;
        private readonly IChangeLogManipulator changeLogManipulator;
        private readonly ICapiCleanUpService capiCleanUpService;
        private readonly IMvxMessenger messenger;
        private readonly ISyncPackageRestoreService packageRestoreService;
        private readonly IExternalAppLauncher externalAppLauncher;

        public string QuestionnaireName { get; private set; }
        public Guid InterviewId { get; private set; }
        public DashboardInterviewStatus Status { get; private set; }
        public List<PrefilledQuestion> PrefilledQuestions { get; private set; }
        public string DateComment { get; private set; }
        public string Comment { get; private set; }
        public bool HasComment { get; private set; }

        public InterviewDashboardItemViewModel(
            IViewModelNavigationService viewModelNavigationService,
            IUserInteractionService userInteractionService,
            IStatefulInterviewRepository interviewRepository,
            ICommandService commandService,
            IPrincipal principal,
            IChangeLogManipulator changeLogManipulator,
            ICapiCleanUpService capiCleanUpService,
            IMvxMessenger messenger, 
            ISyncPackageRestoreService packageRestoreService,
            IExternalAppLauncher externalAppLauncher)
        {
            this.viewModelNavigationService = viewModelNavigationService;
            this.userInteractionService = userInteractionService;
            this.interviewRepository = interviewRepository;
            this.commandService = commandService;
            this.principal = principal;
            this.changeLogManipulator = changeLogManipulator;
            this.capiCleanUpService = capiCleanUpService;
            this.messenger = messenger;
            this.packageRestoreService = packageRestoreService;
            this.externalAppLauncher = externalAppLauncher;
        }

        public void Init(DashboardQuestionnaireItem item)
        {
            this.InterviewId = item.PublicKey;
            this.Status = item.Status;
            this.QuestionnaireName = string.Format(InterviewerUIResources.DashboardItem_Title, item.Title, item.QuestionnaireVersion);
            this.DateComment = this.GetInterviewDateCommentByStatus(item, this.Status);
            this.Comment = this.GetInterviewCommentByStatus(item);
            this.PrefilledQuestions = this.GetTop3PrefilledQuestions(item.Properties);
            this.GpsLocation = item.GpsLocation;
            this.IsSupportedRemove = item.CanBeDeleted;
            this.HasComment = !string.IsNullOrEmpty(this.Comment);
        }

        public GpsCoordinatesViewModel GpsLocation { get; private set; }

        public bool HasGpsLocation
        {
            get { return this.GpsLocation != null; }
        }

        public IMvxCommand NavigateToGpsLocationCommand
        {
            get { return new MvxCommand(this.NavigateToGpsLocation, () => this.HasGpsLocation); }
        }

        private void NavigateToGpsLocation()
        {
            this.externalAppLauncher.LaunchMapsWithTargetLocation(this.GpsLocation.Latitude, this.GpsLocation.Longitude);
        }

        private string GetInterviewDateCommentByStatus(DashboardQuestionnaireItem item, DashboardInterviewStatus status)
        {
            switch (status)
            {
                case DashboardInterviewStatus.New:
                    if (item.InterviewerAssignedDateTime.HasValue)
                        return FormatDateTimeString(InterviewerUIResources.DashboardItem_AssignedOn, item.InterviewerAssignedDateTime);
                    return FormatDateTimeString(InterviewerUIResources.DashboardItem_CreatedOn, item.CreatedDateTime);
                case DashboardInterviewStatus.InProgress:
                    return FormatDateTimeString(InterviewerUIResources.DashboardItem_StartedOn, item.StartedDateTime);
                case DashboardInterviewStatus.Completed:
                    return FormatDateTimeString(InterviewerUIResources.DashboardItem_CompletedOn, item.CompletedDateTime);
                case DashboardInterviewStatus.Rejected:
                    return FormatDateTimeString(InterviewerUIResources.DashboardItem_RejectedOn, item.RejectedDateTime);
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

        private string GetInterviewCommentByStatus(DashboardQuestionnaireItem item)
        {
            switch (item.Status)
            {
                case DashboardInterviewStatus.New:
                    return InterviewerUIResources.DashboardItem_NotStarted;
                case DashboardInterviewStatus.InProgress:
                    return InterviewerUIResources.DashboardItem_InProgress;
                case DashboardInterviewStatus.Completed:
                    return item.Comments;
                case DashboardInterviewStatus.Rejected:
                    return item.Comments;
                default:
                    return string.Empty;
            }
        }

        private List<PrefilledQuestion> GetTop3PrefilledQuestions(IEnumerable<FeaturedItem> featuredItems)
        {
            return featuredItems.Select(fi => new PrefilledQuestion {
                                    Answer = fi.Value,
                                    Question = fi.Title
                                }).Take(3).ToList();
        }

        public bool IsSupportedRemove { get; set; }

        public IMvxCommand RemoveInterviewCommand
        {
            get { return new MvxCommand(this.RemoveInterview); }
        }

        private async void RemoveInterview()
        {
            var isNeedDelete = await this.userInteractionService.ConfirmAsync(
                InterviewerUIResources.Dashboard_RemoveInterviewQuestion.FormatString(this.QuestionnaireName),
                okButton: UIResources.Yes,
                cancelButton: UIResources.No);

            if (!isNeedDelete)
                return;

            capiCleanUpService.DeleteInterview(this.InterviewId);
            RaiseRemovedDashboardItem();
        }

        public IMvxCommand LoadDashboardItemCommand
        {
            get { return new MvxCommand(async () => await this.LoadInterview()); }
        }

        public async Task LoadInterview()
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

                var restartInterviewCommand = new RestartInterviewCommand(this.InterviewId, this.principal.CurrentUserIdentity.UserId, "", DateTime.UtcNow);
                await this.commandService.ExecuteAsync(restartInterviewCommand);
                this.changeLogManipulator.CreateOrReopenDraftRecord(this.InterviewId, this.principal.CurrentUserIdentity.UserId);
            }

            await Task.Run(async () =>
            {
                RaiseStartingLongOperation();
                this.packageRestoreService.CheckAndApplySyncPackage(this.InterviewId);

                var interviewIdString = this.InterviewId.FormatGuid();
                IStatefulInterview interview = interviewRepository.Get(interviewIdString);

                if (interview.CreatedOnClient)
                {
                    await this.viewModelNavigationService.NavigateToPrefilledQuestionsAsync(interviewIdString);
                }
                else
                {
                    await this.viewModelNavigationService.NavigateToInterviewAsync(interviewIdString);
                }
            });
        }

        private void RaiseStartingLongOperation()
        {
            messenger.Publish(new StartingLongOperationMessage(this));
        }

        private void RaiseRemovedDashboardItem()
        {
            messenger.Publish(new RemovedDashboardItemMessage(this));
        }
    }
}