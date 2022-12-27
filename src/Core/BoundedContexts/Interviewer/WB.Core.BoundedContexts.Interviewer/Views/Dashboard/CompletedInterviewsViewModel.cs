using System;
using System.Linq.Expressions;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;
using WB.Core.SharedKernels.Enumerator.Properties;
using WB.Core.SharedKernels.Enumerator.Services;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure.Storage;
using WB.Core.SharedKernels.Enumerator.ViewModels.Dashboard;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails.Groups;
using WB.Core.SharedKernels.Enumerator.Views;

namespace WB.Core.BoundedContexts.Interviewer.Views.Dashboard
{
    public class CompletedInterviewsViewModel : BaseInterviewsViewModel
    {
        public override GroupStatus InterviewStatus => GroupStatus.Completed;
        public override string TabTitle => EnumeratorUIResources.Dashboard_CompletedLinkText;
        public override string TabDescription => EnumeratorUIResources.Dashboard_CompletedTabText;
        
        public event EventHandler<InterviewRemovedArgs>? OnInterviewRemoved;

        public CompletedInterviewsViewModel(
            IPlainStorage<InterviewView> interviewViewRepository,
            IInterviewViewModelFactory viewModelFactory,
            IPlainStorage<PrefilledQuestionView> identifyingQuestionsRepo,
            IPrincipal principal) : base(viewModelFactory, interviewViewRepository, identifyingQuestionsRepo, principal)
        {
        }

        protected override Expression<Func<InterviewView, bool>> GetDbQuery()
        {
            var interviewerId = Principal.CurrentUserIdentity.UserId;

            return interview => interview.ResponsibleId == interviewerId &&
                                (interview.Mode == InterviewMode.CAPI || interview.Mode == null) && 
                                interview.Status == SharedKernels.DataCollection.ValueObjects.Interview
                                    .InterviewStatus.Completed;
        }

        protected override void OnItemCreated(InterviewDashboardItemViewModel interviewDashboardItem)
        {
            base.OnItemCreated(interviewDashboardItem);
            interviewDashboardItem.OnItemRemoved += this.InterviewDashboardItem_OnItemRemoved;
        }

        private async void InterviewDashboardItem_OnItemRemoved(object? sender, System.EventArgs e)
        {
            if (sender == null) return;
            var dashboardItem = (InterviewDashboardItemViewModel) sender;

            this.ItemsCount--;
            this.UpdateTitle();

            await this.InvokeOnMainThreadAsync(() =>
            {
                this.UiItems.Remove(dashboardItem);

            }, false).ConfigureAwait(false);

            this.OnInterviewRemoved?.Invoke(sender,
                new InterviewRemovedArgs(dashboardItem.AssignmentId, dashboardItem.InterviewId));
        }
    }
}
