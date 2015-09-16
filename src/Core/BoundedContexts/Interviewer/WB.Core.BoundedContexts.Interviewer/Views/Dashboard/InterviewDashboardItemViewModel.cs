using System;
using System.Collections.Generic;
using System.Linq;
using Cirrious.MvvmCross.ViewModels;
using WB.Core.BoundedContexts.Interviewer.ChangeLog;
using WB.Core.BoundedContexts.Interviewer.Properties;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.SharedKernels.DataCollection.Commands.Interview;
using WB.Core.SharedKernels.Enumerator.Properties;
using WB.Core.SharedKernels.Enumerator.Services;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure;
using WB.UI.Interviewer.ViewModel.Dashboard;

namespace WB.Core.BoundedContexts.Interviewer.Views.Dashboard
{
    public class InterviewDashboardItemViewModel : IDashboardItem
    {
        private readonly IViewModelNavigationService viewModelNavigationService;
        private readonly IUserInteractionService userInteractionService;
        private readonly ICommandService commandService;
        private readonly IPrincipal principal;
        private readonly IChangeLogManipulator changeLogManipulator;

        public string QuestionariName { get; private set; }
        public Guid InterviewId { get; private set; }
        public DashboardInterviewStatus Status { get; private set; }
        public List<PrefilledQuestion> PrefilledQuestions { get; private set; }
        public string DateComment { get; private set; }
        public string Comment { get; private set; }

        public InterviewDashboardItemViewModel(
            IViewModelNavigationService viewModelNavigationService,
            IUserInteractionService userInteractionService,
            ICommandService commandService,
            IPrincipal principal,
            IChangeLogManipulator changeLogManipulator)
        {
            this.viewModelNavigationService = viewModelNavigationService;
            this.userInteractionService = userInteractionService;
            this.commandService = commandService;
            this.principal = principal;
            this.changeLogManipulator = changeLogManipulator;
        }

        public void Init(DashboardQuestionnaireItem item)
        {
            InterviewId = item.PublicKey;
            Status = item.Status;
            QuestionariName = string.Format("{0} (v{1})", item.Title, item.QuestionnaireVersion);
            DateComment = GetInterviewDateCommentByStatus(item, Status);
            Comment = GetInterviewCommentByStatus(item);
            PrefilledQuestions = this.GetPrefilledQuestions(item.Properties);
            IsSupportedRemove = item.CanBeDeleted;
        }

        private string GetInterviewDateCommentByStatus(DashboardQuestionnaireItem item, DashboardInterviewStatus status)
        {
            switch (status)
            {
                case DashboardInterviewStatus.New:
                    return item.CreatedDateTime.HasValue ? "Created on {0}".FormatString(item.CreatedDateTime) : string.Empty;
                case DashboardInterviewStatus.InProgress:
                    return item.StartedDateTime.HasValue ? "Started on {0}".FormatString(item.StartedDateTime) : string.Empty;
                case DashboardInterviewStatus.Complited:
                case DashboardInterviewStatus.Rejected:
                    return item.ComplitedDateTime.HasValue ? "Complited on {0}".FormatString(item.ComplitedDateTime) : string.Empty;
                default:
                    return string.Empty;
            }
        }

        private string GetInterviewCommentByStatus(DashboardQuestionnaireItem item)
        {
            switch (item.Status)
            {
                case DashboardInterviewStatus.New:
                    return "Not started";
                case DashboardInterviewStatus.InProgress:
                    return string.Empty;
                case DashboardInterviewStatus.Complited:
                    return item.Comments;
                case DashboardInterviewStatus.Rejected:
                    return item.Comments;
                default:
                    return string.Empty;
            }
        }

        private List<PrefilledQuestion> GetPrefilledQuestions(IEnumerable<FeaturedItem> featuredItems)
        {
            return featuredItems.Select(fi => new PrefilledQuestion()
                {
                    Answer = fi.Value,
                    Question = fi.Title
                }).ToList();
        }

        public bool IsSupportedRemove { get; set; }

        public IMvxCommand RemoveInterviewCommand
        {
            get { return new MvxCommand(RemoveInterview); }
        }

        private async void RemoveInterview()
        {
            var isNeedDelete = await userInteractionService.ConfirmAsync(
                InterviewerUIResources.Dashboard_RemoveInterviewQuestion.FormatString(QuestionariName),
                okButton: UIResources.Yes,
                cancelButton: UIResources.No);

            if (!isNeedDelete)
                return;

            var deleteInterviewCommand = new DeleteInterviewCommand(InterviewId, principal.CurrentUserIdentity.UserId);
            await commandService.ExecuteAsync(deleteInterviewCommand);
            changeLogManipulator.CleanUpChangeLogByEventSourceId(InterviewId);
        }

        public IMvxCommand LoadDashboardItemCommand
        {
            get { return new MvxCommand(LoadInterview); }
        }

        private async void LoadInterview()
        {
            if (Status == DashboardInterviewStatus.Complited)
            {
                var isReopen = await userInteractionService.ConfirmAsync(
                    InterviewerUIResources.Dashboard_Reinitialize_Interview_Message,
                    okButton: UIResources.Yes,
                    cancelButton: UIResources.No);

                if (!isReopen)
                {
                    return;
                }

                var restartInterviewCommand = new RestartInterviewCommand(InterviewId, principal.CurrentUserIdentity.UserId, "", DateTime.UtcNow);
                await commandService.ExecuteAsync(restartInterviewCommand);
                changeLogManipulator.CreateOrReopenDraftRecord(InterviewId, principal.CurrentUserIdentity.UserId);
            }

            viewModelNavigationService.NavigateToInterview(InterviewId.FormatGuid());
        }
    }
}