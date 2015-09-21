using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Cirrious.MvvmCross.Plugins.Messenger;
using Cirrious.MvvmCross.Plugins.Network.Droid;
using Cirrious.MvvmCross.ViewModels;
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

        public string QuestionariName { get; private set; }
        public Guid InterviewId { get; private set; }
        public DashboardInterviewStatus Status { get; private set; }
        public List<PrefilledQuestion> PrefilledQuestions { get; private set; }
        public string DateComment { get; private set; }
        public string Comment { get; private set; }

        public InterviewDashboardItemViewModel(
            IViewModelNavigationService viewModelNavigationService,
            IUserInteractionService userInteractionService,
            IStatefulInterviewRepository interviewRepository,
            ICommandService commandService,
            IPrincipal principal,
            IChangeLogManipulator changeLogManipulator,
            ICapiCleanUpService capiCleanUpService,
            IMvxMessenger messenger, 
            ISyncPackageRestoreService packageRestoreService)
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
        }

        public void Init(DashboardQuestionnaireItem item)
        {
            this.InterviewId = item.PublicKey;
            this.Status = item.Status;
            this.QuestionariName = string.Format(InterviewerUIResources.DashboardItem_Title, item.Title, item.QuestionnaireVersion);
            this.DateComment = this.GetInterviewDateCommentByStatus(item, this.Status);
            this.Comment = this.GetInterviewCommentByStatus(item) ?? "-";
            this.PrefilledQuestions = this.GetPrefilledQuestions(item.Properties, 3);
            this.IsSupportedRemove = item.CanBeDeleted;
        }

        private string GetInterviewDateCommentByStatus(DashboardQuestionnaireItem item, DashboardInterviewStatus status)
        {
            switch (status)
            {
                case DashboardInterviewStatus.New:
                    return item.CreatedDateTime.HasValue ? InterviewerUIResources.DashboardItem_CreatedOn.FormatString(item.CreatedDateTime) : string.Empty;
                case DashboardInterviewStatus.InProgress:
                    return item.StartedDateTime.HasValue ? InterviewerUIResources.DashboardItem_StartedOn.FormatString(item.StartedDateTime) : string.Empty;
                case DashboardInterviewStatus.Completed:
                case DashboardInterviewStatus.Rejected:
                    return item.CompletedDateTime.HasValue ? InterviewerUIResources.DashboardItem_CompletedOn.FormatString(item.CompletedDateTime) : string.Empty;
                default:
                    return string.Empty;
            }
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

        private List<PrefilledQuestion> GetPrefilledQuestions(IEnumerable<FeaturedItem> featuredItems, int count)
        {
            return featuredItems.Select(fi => new PrefilledQuestion()
                {
                    Answer = fi.Value,
                    Question = fi.Title
                }).Take(count).ToList();
        }

        public bool IsSupportedRemove { get; set; }

        public IMvxCommand RemoveInterviewCommand
        {
            get { return new MvxCommand(this.RemoveInterview); }
        }

        private async void RemoveInterview()
        {
            var isNeedDelete = await this.userInteractionService.ConfirmAsync(
                InterviewerUIResources.Dashboard_RemoveInterviewQuestion.FormatString(this.QuestionariName),
                okButton: UIResources.Yes,
                cancelButton: UIResources.No);

            if (!isNeedDelete)
                return;

            capiCleanUpService.DeleteInterview(this.InterviewId);
            RaiseRemovedDashboardItem();
        }

        public IMvxCommand LoadDashboardItemCommand
        {
            get { return new MvxCommand(this.LoadInterview); }
        }

        private async void LoadInterview()
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

            await Task.Run(() =>
            {
                RaiseStartingLongOperation();
                this.packageRestoreService.CheckAndApplySyncPackage(this.InterviewId);

                var interviewIdString = this.InterviewId.FormatGuid();
                IStatefulInterview interview = interviewRepository.Get(interviewIdString);

                if (interview.CreatedOnClient)
                {
                    this.viewModelNavigationService.NavigateToPrefilledQuestions(interviewIdString);
                }
                else
                {
                    this.viewModelNavigationService.NavigateToInterview(interviewIdString);
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